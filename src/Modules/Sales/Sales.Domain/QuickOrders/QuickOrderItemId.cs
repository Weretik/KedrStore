namespace Sales.Domain.QuickOrders;

public readonly record struct QuickOrderItemId
{
    public long Value { get; }

    private QuickOrderItemId(long value) => Value = value;

    // EF Core uses negative temporary keys before PostgreSQL assigns the real identity value.
    public static QuickOrderItemId FromStorage(long value) => new(value);
}
