namespace Content.Domain.Home;

/// <summary>Database identity of a home page question.</summary>
public readonly record struct HomeFaqItemId : IComparable<HomeFaqItemId>
{
    public long Value { get; }

    private HomeFaqItemId(long value) => Value = value;

    public static HomeFaqItemId Create(long value) => value > 0
        ? new HomeFaqItemId(value)
        : throw new ArgumentOutOfRangeException(nameof(value));

    public static HomeFaqItemId FromStorage(long value) => new(value);

    public int CompareTo(HomeFaqItemId other) => Value.CompareTo(other.Value);
}
