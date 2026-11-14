namespace Media.Domain.Files;

/// <summary>Database identity of a library file; other modules link to files by this ID.</summary>
public readonly record struct MediaFileId : IComparable<MediaFileId>
{
    public long Value { get; }

    private MediaFileId(long value) => Value = value;

    public static MediaFileId Create(long value) => value > 0
        ? new MediaFileId(value)
        : throw new ArgumentOutOfRangeException(nameof(value));

    public static MediaFileId FromStorage(long value) => new(value);

    public int CompareTo(MediaFileId other) => Value.CompareTo(other.Value);
}
