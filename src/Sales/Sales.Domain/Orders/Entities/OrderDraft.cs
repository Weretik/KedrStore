using Sales.Domain.Orders.ValueObjects;

namespace Sales.Domain.Orders.Entities;

public sealed class OrderDraft
{
    private readonly List<OrderDraftLine> _lines = [];
    public OrderDraftId Id { get; private set; }
    public string CounterpartyId { get; private set; } = null!;
    public string Title { get; private set; } = null!;
    public string? Note { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public string Version { get; private set; } = null!;
    public IReadOnlyCollection<OrderDraftLine> Lines => _lines.AsReadOnly();

    private OrderDraft() { }
    private OrderDraft(string title, string? note, string counterpartyId,
        IEnumerable<OrderDraftLine> lines, DateTimeOffset now)
    {
        SetFields(title, note, counterpartyId, lines);
        CreatedAtUtc = now;
        UpdatedAtUtc = now;
        Version = Guid.NewGuid().ToString("N");
    }

    public static OrderDraft Create(string title, string? note, string counterpartyId,
        IEnumerable<OrderDraftLine> lines, DateTimeOffset now)
        => new(title, note, counterpartyId, lines, now);

    public void Update(string title, string? note, string counterpartyId,
        IEnumerable<OrderDraftLine> lines, DateTimeOffset now)
    {
        SetFields(title, note, counterpartyId, lines);
        UpdatedAtUtc = now;
        Version = Guid.NewGuid().ToString("N");
    }

    private void SetFields(string title, string? note, string counterpartyId, IEnumerable<OrderDraftLine> lines)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length > 200)
            throw new ArgumentException("Draft title is invalid.", nameof(title));
        if (string.IsNullOrWhiteSpace(counterpartyId) || counterpartyId.Trim().Length > 64)
            throw new ArgumentException("Counterparty ID is invalid.", nameof(counterpartyId));
        var values = lines.ToArray();
        if (values.Length is < 1 or > 100 || values.Select(line => line.ProductId).Distinct(StringComparer.Ordinal).Count() != values.Length)
            throw new ArgumentException("Draft lines must contain 1-100 unique products.", nameof(lines));
        var normalizedNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        if (normalizedNote?.Length > 2_000)
            throw new ArgumentException("Draft note is too long.", nameof(note));
        Title = title.Trim();
        Note = normalizedNote;
        CounterpartyId = counterpartyId.Trim();
        _lines.Clear();
        _lines.AddRange(values);
    }
}