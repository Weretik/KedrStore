namespace Sales.Domain.QuickOrders;

/// <summary>The id of a site order. The site's <c>Idempotency-Key</c> becomes it, so a repeat finds the same order.</summary>
public readonly record struct QuickOrderId
{
    public Guid Value { get; }

    private QuickOrderId(Guid value) => Value = value;

    public static QuickOrderId Create(Guid value) => value == Guid.Empty
        ? throw new ArgumentOutOfRangeException(nameof(value), value, "Quick order ID must not be empty.")
        : new QuickOrderId(value);

    public static QuickOrderId New(DateTimeOffset now) => new(Guid.CreateVersion7(now));
}
