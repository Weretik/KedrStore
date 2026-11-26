
using Sales.Domain.Orders.ValueObjects;

namespace Sales.Domain.QuickOrders;

/// <summary>
/// An order a buyer sent from the site without an account (spec sales/009). It is kept whether or not Telegram got it,
/// and it carries how its delivery to the managers' Telegram chat went.
/// </summary>
public sealed class QuickOrder : BaseEntity<QuickOrderId>, IAggregateRoot
{
    public const int FirstNameMaxLength = 30;
    public const int PhoneMaxLength = 20;
    /// <summary>Below the 1C order comment limit, so the order line with the buyer's comment always fits (R-010).</summary>
    public const int CommentMaxLength = 800;
    public const int TelegramErrorMaxLength = 1_000;

    private readonly List<QuickOrderItem> _items = [];

    public string FirstName { get; private set; } = null!;
    public string Phone { get; private set; } = null!;

    /// <summary>What the buyer wrote for the managers; trimmed, null when empty. It goes to Telegram and the 1C order comment.</summary>
    public string? Comment { get; private set; }

    public DateTimeOffset PlacedAtUtc { get; private set; }

    /// <summary>Σ unit price × quantity as the buyer saw it; a copy for the managers, not a charge.</summary>
    public decimal Total { get; private set; }

    public IReadOnlyCollection<QuickOrderItem> Items => _items.AsReadOnly();

    public DateTimeOffset? TelegramSentAtUtc { get; private set; }
    public DateTimeOffset? TelegramLastAttemptAtUtc { get; private set; }
    public string? TelegramLastError { get; private set; }

    /// <summary>Set when the outbox gave up; only then may a person send the order again.</summary>
    public DateTimeOffset? TelegramFailedAtUtc { get; private set; }

    /// <summary>
    /// The outbox message that turns this order into a 1C order (spec sales/009, R-010); stable across retries. Null
    /// for orders placed before site orders went to 1C: they are never sent.
    /// </summary>
    public Guid? OneCOrderMessageId { get; private set; }

    /// <summary>The order made from this one for 1C; set once, so a repeated message makes no second order.</summary>
    public OrderId? OneCOrderId { get; private set; }

    public QuickOrderTelegramStatus TelegramStatus => TelegramSentAtUtc is not null
        ? QuickOrderTelegramStatus.Sent
        : TelegramFailedAtUtc is not null ? QuickOrderTelegramStatus.Failed : QuickOrderTelegramStatus.Pending;

    private QuickOrder() { }

    public static DomainResult<QuickOrder> Create(
        QuickOrderId id, string firstName, string phone, IEnumerable<QuickOrderItem> items, DateTimeOffset placedAtUtc,
        string? comment = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            return QuickOrderErrors.FirstNameRequired();
        if (string.IsNullOrWhiteSpace(phone))
            return QuickOrderErrors.PhoneRequired();
        var orderItems = items.ToList();
        if (orderItems.Count == 0)
            return QuickOrderErrors.ItemsRequired();
        var trimmedComment = string.IsNullOrWhiteSpace(comment) ? null : comment.Trim();
        if (trimmedComment is { Length: > CommentMaxLength })
            return QuickOrderErrors.CommentTooLong();

        var order = new QuickOrder
        {
            Id = id,
            FirstName = firstName.Trim(),
            Phone = phone.Trim(),
            Comment = trimmedComment,
            PlacedAtUtc = placedAtUtc,
            Total = orderItems.Sum(item => item.Amount),
            OneCOrderMessageId = Guid.CreateVersion7(placedAtUtc),
        };
        order._items.AddRange(orderItems);
        return order;
    }

    /// <summary>The same buyer and the same lines in the same order: <paramref name="other"/> repeats this order.</summary>
    public bool SameAs(QuickOrder other) =>
        FirstName == other.FirstName
        && Phone == other.Phone
        && Comment == other.Comment
        && _items.Count == other._items.Count
        && _items.Zip(other._items).All(pair => pair.First.SameAs(pair.Second));

    /// <summary>The 1C order made from this one is stored (R-010); a second, different order is refused.</summary>
    public DomainResult LinkOneCOrder(OrderId orderId)
    {
        if (OneCOrderId is { } linked && linked != orderId)
            return QuickOrderErrors.OneCOrderAlreadyLinked();

        OneCOrderId = orderId;
        return DomainResult.Success();
    }

    /// <summary>Telegram has the order. A later attempt (a repeat delivery) changes nothing.</summary>
    public void MarkSentToTelegram(DateTimeOffset now)
    {
        TelegramLastAttemptAtUtc = now;
        TelegramSentAtUtc ??= now;
        TelegramLastError = null;
        TelegramFailedAtUtc = null;
    }

    /// <summary>One delivery attempt failed; the outbox tries again.</summary>
    public void RecordTelegramFailure(string error, DateTimeOffset now)
    {
        if (TelegramSentAtUtc is not null)
            return;
        TelegramLastAttemptAtUtc = now;
        TelegramLastError = Shorten(error);
    }

    /// <summary>The outbox made its last attempt; the order waits for a person to send it again.</summary>
    public void GiveUpTelegram(string? error, DateTimeOffset now)
    {
        if (TelegramSentAtUtc is not null)
            return;
        TelegramFailedAtUtc = now;
        if (!string.IsNullOrWhiteSpace(error))
            TelegramLastError = Shorten(error);
    }

    /// <summary>A person asks to send a failed order again; the caller stores a new outbox message.</summary>
    public DomainResult RequestTelegramResend()
    {
        if (TelegramSentAtUtc is not null)
            return QuickOrderErrors.AlreadySent();
        if (TelegramFailedAtUtc is null)
            return QuickOrderErrors.DeliveryInProgress();

        TelegramFailedAtUtc = null;
        TelegramLastError = null;
        return DomainResult.Success();
    }

    private static string Shorten(string error)
    {
        var text = string.IsNullOrWhiteSpace(error) ? "Delivery failed." : error.Trim();
        return text.Length <= TelegramErrorMaxLength ? text : text[..TelegramErrorMaxLength];
    }
}
