namespace Sales.Domain.Orders.ValueObjects;

public readonly record struct OneCOrderSyncId
{
    public long Value { get; }

    private OneCOrderSyncId(long value) => Value = value;

    public static OneCOrderSyncId Create(long value)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(nameof(value), value, "1C order synchronization ID must be positive.");

        return new OneCOrderSyncId(value);
    }
}
