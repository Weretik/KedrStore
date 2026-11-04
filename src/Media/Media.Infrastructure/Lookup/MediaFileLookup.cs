using Media.Application.Contracts.Storage;
using Media.Contracts.Files;

namespace Media.Infrastructure.Lookup;

internal sealed class MediaFileLookup(MediaDbContext db, IMediaUrlBuilder urls) : IMediaFileLookup
{
    public async Task<IReadOnlyDictionary<long, MediaFileReference>> GetFilesAsync(
        IReadOnlyCollection<long> ids, CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
            return new Dictionary<long, MediaFileReference>();

        var fileIds = ids.Distinct().Where(id => id > 0).Select(MediaFileId.FromStorage).ToList();
        var files = await db.MediaFiles.AsNoTracking()
            .Where(file => fileIds.Contains(file.Id))
            .Select(file => new { file.Id, file.FileName, file.StorageKey, file.Width, file.Height })
            .ToListAsync(cancellationToken);

        return files.ToDictionary(
            file => file.Id.Value,
            file => new MediaFileReference(file.Id.Value, file.FileName, urls.Build(file.StorageKey), file.Width, file.Height));
    }
}
