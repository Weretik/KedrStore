namespace Content.Domain.Seo;

/// <summary>Database identity of the site SEO settings row.</summary>
public readonly record struct SiteSeoSettingsId : IComparable<SiteSeoSettingsId>
{
    public int Value { get; }

    private SiteSeoSettingsId(int value) => Value = value;

    public static SiteSeoSettingsId Create(int value) => value > 0
        ? new SiteSeoSettingsId(value)
        : throw new ArgumentOutOfRangeException(nameof(value));

    public static SiteSeoSettingsId FromStorage(int value) => new(value);

    public int CompareTo(SiteSeoSettingsId other) => Value.CompareTo(other.Value);
}
