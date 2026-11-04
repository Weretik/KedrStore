using Media.Application.Contracts.Storage;
using Media.Contracts.Files;

namespace Media.Infrastructure.Files;

/// <summary>Serves <see cref="IMediaFileDirectory"/> from the library tables; URLs are built at read time.</summary>
internal sealed class MediaFileDirectory(IReadMediaDbContext db, IMediaUrlBuilder urls) : IMediaFileDirectory
{
    public async Task<IReadOnlyList<MediaFileReference>> GetByIdsAsync(IReadOnlyCollection<long> ids,
        CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
            return [];

        var fileIds = ids.Where(id => id > 0).Distinct().Select(MediaFileId.Create).ToArray();
        var rows = await db.MediaFiles.AsNoTracking()
            .Where(file => fileIds.Contains(file.Id))
            .Select(file => new { file.Id, file.StorageKey, file.FileName })
            .ToListAsync(cancellationToken);

        return rows.Select(row => ToReference(row.Id, row.StorageKey, row.FileName)).ToArray();
    }

    public async Task<IReadOnlyList<MediaFileReference>> ListInStorageDirectoryAsync(string directory,
        CancellationToken cancellationToken)
    {
        var prefix = directory.Trim().Trim('/') + "/";
        var rows = await db.MediaFiles.AsNoTracking()
            .Where(file => file.StorageKey.StartsWith(prefix))
            .Select(file => new { file.Id, file.StorageKey, file.FileName })
            .ToListAsync(cancellationToken);

        return rows
            .Where(row => row.StorageKey.IndexOf('/', prefix.Length) < 0)
            .Select(row => ToReference(row.Id, row.StorageKey, row.FileName))
            .ToArray();
    }

    private MediaFileReference ToReference(MediaFileId id, string storageKey, string fileName)
        => new(id.Value, storageKey, fileName, urls.Build(storageKey));
}
