namespace Content.Domain.Redirects;

/// <summary>Database identity of a site redirect.</summary>
public readonly record struct SiteRedirectId : IComparable<SiteRedirectId>
{
    public long Value { get; }

    private SiteRedirectId(long value) => Value = value;

    public static SiteRedirectId Create(long value) => value > 0
        ? new SiteRedirectId(value)
        : throw new ArgumentOutOfRangeException(nameof(value));

    public static SiteRedirectId FromStorage(long value) => new(value);

    public int CompareTo(SiteRedirectId other) => Value.CompareTo(other.Value);
}
