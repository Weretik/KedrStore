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

    public static MediaFolder Create(string name, MediaFolderId? parentId, DateTimeOffset now)
        => new() { Name = NormalizeName(name), ParentId = parentId, CreatedAt = now, UpdatedAt = now };

    public void Rename(string name, DateTimeOffset now)
    {
        Name = NormalizeName(name);
        UpdatedAt = now;
    }

    /// <summary>The caller checks that <paramref name="parentId"/> is not this folder's descendant.</summary>
    public void MoveTo(MediaFolderId? parentId, DateTimeOffset now)
    {
        if (parentId == Id)
            throw new DomainException(MediaErrors.FolderMoveIntoItself());
        ParentId = parentId;
        UpdatedAt = now;
    }

    public static string NormalizeName(string? name)
    {
        var normalized = name?.Trim() ?? string.Empty;
        if (normalized.Length == 0)
            throw new DomainException(MediaErrors.Required("Name"));
        if (normalized.Length > NameMaxLength)
            throw new DomainException(MediaErrors.TooLong("Name", NameMaxLength, normalized.Length));
        return normalized;
    }
}
