namespace Content.Domain.Wholesale;

/// <summary>Database identity of the wholesale page row.</summary>
public readonly record struct WholesalePageId : IComparable<WholesalePageId>
{
    public int Value { get; }

    private WholesalePageId(int value) => Value = value;

    public static WholesalePageId Create(int value) => value > 0
        ? new WholesalePageId(value)
        : throw new ArgumentOutOfRangeException(nameof(value));

    public static WholesalePageId FromStorage(int value) => new(value);

    public int CompareTo(WholesalePageId other) => Value.CompareTo(other.Value);
}
