namespace Content.Domain.Home;

/// <summary>Database identity of a home page brand.</summary>
public readonly record struct HomeBrandId : IComparable<HomeBrandId>
{
    public long Value { get; }

    private HomeBrandId(long value) => Value = value;

    public static HomeBrandId Create(long value) => value > 0
        ? new HomeBrandId(value)
        : throw new ArgumentOutOfRangeException(nameof(value));

    public static HomeBrandId FromStorage(long value) => new(value);

    public int CompareTo(HomeBrandId other) => Value.CompareTo(other.Value);
}
