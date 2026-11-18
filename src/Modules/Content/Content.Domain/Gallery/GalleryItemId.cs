namespace Content.Domain.Gallery;

/// <summary>Database identity of a gallery item.</summary>
public readonly record struct GalleryItemId : IComparable<GalleryItemId>
{
    public long Value { get; }

    private GalleryItemId(long value) => Value = value;

    public static GalleryItemId Create(long value) => value > 0
        ? new GalleryItemId(value)
        : throw new ArgumentOutOfRangeException(nameof(value));

    public static GalleryItemId FromStorage(long value) => new(value);

    public int CompareTo(GalleryItemId other) => Value.CompareTo(other.Value);
}
