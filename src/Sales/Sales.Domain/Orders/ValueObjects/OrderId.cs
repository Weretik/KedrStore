namespace Sales.Domain.Orders.ValueObjects;

public readonly record struct OrderId
{
    public long Value { get; }

    private OrderId(long value) => Value = value;

    public static OrderId Create(long value)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(nameof(value), value, "Order ID must be positive.");

        return new OrderId(value);
    }

    // EF Core uses negative temporary keys before PostgreSQL assigns the real identity value.
    public static OrderId FromStorage(long value) => new(value);
}
