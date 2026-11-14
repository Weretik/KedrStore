namespace Sales.Domain.Orders.ValueObjects;

public readonly record struct OrderDraftLineId
{
    public long Value { get; }
    private OrderDraftLineId(long value) => Value = value;
    public static OrderDraftLineId FromStorage(long value) => new(value);
}