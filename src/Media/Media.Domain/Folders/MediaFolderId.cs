namespace Media.Domain.Folders;

/// <summary>Database identity of a library folder.</summary>
public readonly record struct MediaFolderId : IComparable<MediaFolderId>
{
    public long Value { get; }

    private MediaFolderId(long value) => Value = value;

    public static MediaFolderId Create(long value) => value > 0
        ? new MediaFolderId(value)
        : throw new ArgumentOutOfRangeException(nameof(value));

    public static MediaFolderId FromStorage(long value) => new(value);

    public int CompareTo(MediaFolderId other) => Value.CompareTo(other.Value);
}
