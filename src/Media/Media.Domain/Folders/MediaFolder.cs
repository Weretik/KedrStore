namespace Media.Domain.Folders;

/// <summary>
/// A virtual folder of the media library. Folders live only in the database, so moving a file or a folder
/// never moves anything on the CDN; <c>null</c> <see cref="ParentId"/> means the library root.
/// </summary>
public sealed class MediaFolder : IAggregateRoot
{
    public const int NameMaxLength = 100;

    public MediaFolderId Id { get; private set; }
    public string Name { get; private set; } = null!;
    public MediaFolderId? ParentId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private MediaFolder() { }

    public static DomainResult<MediaFolder> Create(string name, MediaFolderId? parentId, DateTimeOffset now)
    {
        var normalized = NormalizeName(name);
        if (normalized.IsFailure)
            return normalized.Error!;
        return new MediaFolder { Name = normalized.Value, ParentId = parentId, CreatedAt = now, UpdatedAt = now };
    }

    public DomainResult Rename(string name, DateTimeOffset now)
    {
        var normalized = NormalizeName(name);
        if (normalized.IsFailure)
            return normalized.Error!;
        Name = normalized.Value;
        UpdatedAt = now;
        return DomainResult.Success();
    }

    /// <summary>The caller checks that <paramref name="parentId"/> is not this folder's descendant.</summary>
    public DomainResult MoveTo(MediaFolderId? parentId, DateTimeOffset now)
    {
        if (parentId == Id)
            return MediaErrors.FolderMoveIntoItself();
        ParentId = parentId;
        UpdatedAt = now;
        return DomainResult.Success();
    }

    public static DomainResult<string> NormalizeName(string? name)
    {
        var normalized = name?.Trim() ?? string.Empty;
        if (normalized.Length == 0)
            return MediaErrors.Required("Name");
        if (normalized.Length > NameMaxLength)
            return MediaErrors.TooLong("Name", NameMaxLength, normalized.Length);
        return normalized;
    }
}
