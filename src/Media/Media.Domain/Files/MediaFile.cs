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

    public static MediaFile Create(MediaFolderId? folderId, string fileName, MediaFileContent content, DateTimeOffset now)
    {
        var file = new MediaFile { FolderId = folderId, FileName = NormalizeFileName(fileName), CreatedAt = now };
        file.SetContent(content, now);
        return file;
    }

    /// <summary>Points the file at a new picture and returns the old CDN key, which the caller deletes.</summary>
    public string Replace(MediaFileContent content, DateTimeOffset now)
    {
        var previousKey = StorageKey;
        SetContent(content, now);
        return previousKey;
    }

    public void Rename(string fileName, DateTimeOffset now)
    {
        FileName = NormalizeFileName(fileName);
        UpdatedAt = now;
    }

    public void MoveTo(MediaFolderId? folderId, DateTimeOffset now)
    {
        FolderId = folderId;
        UpdatedAt = now;
    }

    public static string NormalizeFileName(string? fileName)
    {
        var normalized = fileName?.Trim() ?? string.Empty;
        if (normalized.Length == 0)
            throw new DomainException(MediaErrors.Required("FileName"));
        if (normalized.Length > FileNameMaxLength)
            throw new DomainException(MediaErrors.TooLong("FileName", FileNameMaxLength, normalized.Length));
        return normalized;
    }

    private void SetContent(MediaFileContent content, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(content);
        var key = content.StorageKey?.Trim().TrimStart('/') ?? string.Empty;
        if (key.Length == 0)
            throw new DomainException(MediaErrors.Required("StorageKey"));
        if (key.Length > StorageKeyMaxLength)
            throw new DomainException(MediaErrors.TooLong("StorageKey", StorageKeyMaxLength, key.Length));
        if (string.IsNullOrWhiteSpace(content.ContentType))
            throw new DomainException(MediaErrors.Required("ContentType"));
        if (content.SizeBytes < 0 || content.Width <= 0 || content.Height <= 0)
            throw new DomainException(MediaErrors.ContentInvalid());

        StorageKey = key;
        ContentType = content.ContentType.Trim();
        SizeBytes = content.SizeBytes;
        Width = content.Width;
        Height = content.Height;
        UpdatedAt = now;
    }
}
