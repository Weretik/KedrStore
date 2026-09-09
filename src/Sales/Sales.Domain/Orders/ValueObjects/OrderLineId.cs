namespace Sales.Domain.Orders.ValueObjects;

public readonly record struct OrderLineId
{
    public long Value { get; }

    private OrderLineId(long value) => Value = value;

    public static OrderLineId Create(long value)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(nameof(value), value, "Order line ID must be positive.");

        return new OrderLineId(value);
    }

    // EF Core uses negative temporary keys before PostgreSQL assigns the real identity value.
    public static OrderLineId FromStorage(long value) => new(value);
}
