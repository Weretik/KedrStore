namespace Media.Application.Features.Folders.Common;

/// <summary>Folder checks that need the other folders: the folder exists, no name clash, no cycle.</summary>
internal static class MediaFolderRules
{
    public const string NameTakenMessage = "A folder with this name already exists here.";

    /// <summary>Returns an error result when the target parent is missing or the name is taken there.</summary>
    public static async Task<Result?> CheckPlacementAsync(IReadMediaDbContext db, long? parentId, string name,
        long? exceptFolderId, CancellationToken cancellationToken)
    {
        var missing = await CheckExistsAsync(db, parentId, "parentId", cancellationToken);
        if (missing is not null)
            return missing;

        MediaFolderId? parent = parentId is { } value ? MediaFolderId.Create(value) : null;

        var lowered = MediaFolder.NormalizeName(name).ToLower();
        var taken = await db.MediaFolders.AnyAsync(folder =>
            folder.ParentId == parent &&
            (exceptFolderId == null || folder.Id != MediaFolderId.FromStorage(exceptFolderId.Value)) &&
            folder.Name.ToLower() == lowered, cancellationToken);
        return taken ? Result.Conflict(NameTakenMessage) : null;
    }

    /// <summary>Returns an invalid result when <paramref name="folderId"/> is given and no such folder exists.</summary>
    public static async Task<Result?> CheckExistsAsync(IReadMediaDbContext db, long? folderId, string field,
        CancellationToken cancellationToken)
    {
        if (folderId is not { } value)
            return null;
        var id = MediaFolderId.Create(value);
        return await db.MediaFolders.AnyAsync(folder => folder.Id == id, cancellationToken)
            ? null
            : Result.Invalid(new ValidationError(field, "The folder does not exist."));
    }

    /// <summary><c>true</c> when <paramref name="candidateParentId"/> is the folder itself or one of its subfolders.</summary>
    public static async Task<bool> IsSelfOrDescendantAsync(IReadMediaDbContext db, MediaFolderId folderId,
        long? candidateParentId, CancellationToken cancellationToken)
    {
        if (candidateParentId is null)
            return false;

        var parents = await db.MediaFolders.AsNoTracking()
            .Select(folder => new { folder.Id, folder.ParentId })
            .ToDictionaryAsync(folder => folder.Id, folder => folder.ParentId, cancellationToken);

        MediaFolderId? current = MediaFolderId.Create(candidateParentId.Value);
        var visited = new HashSet<MediaFolderId>();
        while (current is { } id && visited.Add(id))
        {
            if (id == folderId)
                return true;
            current = parents.GetValueOrDefault(id);
        }
        return false;
    }
}
