namespace Sales.Domain.Orders.ValueObjects;

public readonly record struct OrderDraftId
{
    public long Value { get; }
    private OrderDraftId(long value) => Value = value;
    public static OrderDraftId Create(long value) => value > 0
        ? new OrderDraftId(value)
        : throw new ArgumentOutOfRangeException(nameof(value));
    public static OrderDraftId FromStorage(long value) => new(value);
}