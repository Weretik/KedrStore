using Sales.Domain.Customers;
using Sales.Domain.Orders;
using Sales.Domain.Orders.ValueObjects;
using Sales.Domain.Orders.Errors;

namespace Sales.Domain.Orders;

public sealed class Order : BaseAuditableEntity<OrderId>, IAggregateRoot, IAuditableEntity
{
    public const int OrderNumberMaxLength = 32;
    public const int CommentMaxLength = 1_000;
    public const int OneCDocumentNumberMaxLength = 128;
    public const int OneCRejectionMaxLength = 1_000;

    private readonly List<OrderLine> _lines = [];

    public string OrderNumber { get; private set; } = null!;
    public CounterpartyId CounterpartyId { get; private set; }
    public string? Comment { get; private set; }
    public IReadOnlyCollection<OrderLine> Lines => _lines.AsReadOnly();

    /// <summary>The outbox message that delivers the order to 1C (platform spec 006, R-010); stable across retries.</summary>
    public Guid OneCMessageId { get; private set; }

    /// <summary>The 1C document of an accepted order; it never changes once set.</summary>
    public string? OneCDocumentNumber { get; private set; }
    public DateTimeOffset? OneCAcceptedAtUtc { get; private set; }

    /// <summary>Why 1C refused the order (a business error, not retried by itself).</summary>
    public string? OneCRejection { get; private set; }
    public DateTimeOffset? OneCRejectedAtUtc { get; private set; }

    private Order() { }

    public static DomainResult<Order> Create(string orderNumber, string counterpartyId, string? comment,
        IEnumerable<OrderLine> lines, DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(orderNumber))
            return OrderErrors.OrderNumberRequired();
        if (orderNumber.Trim().Length > OrderNumberMaxLength)
            return OrderErrors.OrderNumberTooLong();
        if (string.IsNullOrWhiteSpace(counterpartyId))
            return OrderErrors.CounterpartyIdRequired();
        var normalizedComment = comment?.Trim();
        if (normalizedComment?.Length > CommentMaxLength)
            return OrderErrors.CommentTooLong();
        var orderLines = lines.ToList();
        if (orderLines.Count == 0)
            return OrderErrors.LinesRequired();

        var order = new Order
        {
            OrderNumber = orderNumber.Trim(),
            CounterpartyId = CounterpartyId.Create(counterpartyId),
            Comment = string.IsNullOrEmpty(normalizedComment) ? null : normalizedComment,
            OneCMessageId = Guid.CreateVersion7(createdAt),
        };
        order._lines.AddRange(orderLines);
        order.MarkAsCreated(createdAt);
        return order;
    }

    public DomainResult AssignOrderNumber(string orderNumber)
    {
        if (!OrderNumber.StartsWith("P", StringComparison.Ordinal))
            throw new InvalidOperationException("Order number is already assigned.");
        if (string.IsNullOrWhiteSpace(orderNumber))
            return OrderErrors.OrderNumberRequired();

        OrderNumber = orderNumber.Trim();
        return DomainResult.Success();
    }

    /// <summary>1C accepted the order and created its document (R-011).</summary>
    public DomainResult AcceptByOneC(string? documentNumber, DateTimeOffset now)
    {
        var number = documentNumber?.Trim();
        if (string.IsNullOrEmpty(number))
            return OrderErrors.OneCDocumentNumberRequired();
        if (number.Length > OneCDocumentNumberMaxLength)
            number = number[..OneCDocumentNumberMaxLength];
        if (OneCDocumentNumber is not null && !string.Equals(OneCDocumentNumber, number, StringComparison.Ordinal))
            return OrderErrors.OneCDocumentNumberChanged();

        OneCDocumentNumber = number;
        OneCAcceptedAtUtc ??= now;
        OneCRejection = null;
        OneCRejectedAtUtc = null;
        MarkAsUpdated(now);
        return DomainResult.Success();
    }

    /// <summary>1C refused the order; the reason stays until a manager retries (R-011).</summary>
    public DomainResult RejectByOneC(string? reason, DateTimeOffset now)
    {
        if (OneCDocumentNumber is not null)
            return OrderErrors.AlreadyAcceptedByOneC();

        var text = string.IsNullOrWhiteSpace(reason) ? "1С відхилила замовлення без пояснення." : reason.Trim();
        OneCRejection = text.Length <= OneCRejectionMaxLength ? text : text[..OneCRejectionMaxLength];
        OneCRejectedAtUtc = now;
        MarkAsUpdated(now);
        return DomainResult.Success();
    }

    /// <summary>A manager sends the order to 1C again (R-013): the old refusal no longer applies.</summary>
    public DomainResult ReopenOneCDelivery(DateTimeOffset now)
    {
        if (OneCDocumentNumber is not null)
            return OrderErrors.AlreadyAcceptedByOneC();

        OneCRejection = null;
        OneCRejectedAtUtc = null;
        MarkAsUpdated(now);
        return DomainResult.Success();
    }
}
