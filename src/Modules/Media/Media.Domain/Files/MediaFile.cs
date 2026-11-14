using Media.Domain.Folders;

namespace Media.Domain.Files;

/// <summary>
/// A picture on the CDN, registered in the library. <see cref="StorageKey"/> is the CDN path from its root;
/// the public URL is built from it at read time. Replacing the picture keeps <see cref="Id"/>, so links stay.
/// </summary>
public sealed class MediaFile : IAggregateRoot
{
    public const int StorageKeyMaxLength = 500;
    public const int FileNameMaxLength = 200;
    public const int ContentTypeMaxLength = 100;

    public MediaFileId Id { get; private set; }
    public MediaFolderId? FolderId { get; private set; }
    public string FileName { get; private set; } = null!;
    public string StorageKey { get; private set; } = null!;
    public string ContentType { get; private set; } = null!;
    public long SizeBytes { get; private set; }
    public int? Width { get; private set; }
    public int? Height { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private MediaFile() { }

    public static DomainResult<MediaFile> Create(MediaFolderId? folderId, string fileName, MediaFileContent content,
        DateTimeOffset now)
    {
        var name = NormalizeFileName(fileName);
        if (name.IsFailure)
            return name.Error!;
        var file = new MediaFile { FolderId = folderId, FileName = name.Value, CreatedAt = now };
        var set = file.SetContent(content, now);
        return set.IsSuccess ? file : set.Error!;
    }

    /// <summary>Points the file at a new picture and returns the old CDN key, which the caller deletes.</summary>
    public DomainResult<string> Replace(MediaFileContent content, DateTimeOffset now)
    {
        var previousKey = StorageKey;
        var set = SetContent(content, now);
        return set.IsSuccess ? previousKey : set.Error!;
    }

    public DomainResult Rename(string fileName, DateTimeOffset now)
    {
        var name = NormalizeFileName(fileName);
        if (name.IsFailure)
            return name.Error!;
        FileName = name.Value;
        UpdatedAt = now;
        return DomainResult.Success();
    }

    public void MoveTo(MediaFolderId? folderId, DateTimeOffset now)
    {
        FolderId = folderId;
        UpdatedAt = now;
    }

    public static DomainResult<string> NormalizeFileName(string? fileName)
    {
        var normalized = fileName?.Trim() ?? string.Empty;
        if (normalized.Length == 0)
            return MediaErrors.Required("FileName");
        if (normalized.Length > FileNameMaxLength)
            return MediaErrors.TooLong("FileName", FileNameMaxLength, normalized.Length);
        return normalized;
    }

    private DomainResult SetContent(MediaFileContent content, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(content);
        var key = content.StorageKey?.Trim().TrimStart('/') ?? string.Empty;
        if (key.Length == 0)
            return MediaErrors.Required("StorageKey");
        if (key.Length > StorageKeyMaxLength)
            return MediaErrors.TooLong("StorageKey", StorageKeyMaxLength, key.Length);
        if (string.IsNullOrWhiteSpace(content.ContentType))
            return MediaErrors.Required("ContentType");
        if (content.SizeBytes < 0 || content.Width <= 0 || content.Height <= 0)
            return MediaErrors.ContentInvalid();

        StorageKey = key;
        ContentType = content.ContentType.Trim();
        SizeBytes = content.SizeBytes;
        Width = content.Width;
        Height = content.Height;
        UpdatedAt = now;
        return DomainResult.Success();
    }
}
