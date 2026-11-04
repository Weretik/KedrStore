using Media.Application.Features.Folders.Common;

namespace Media.Application.Features.Folders.GetTree;

public sealed class GetMediaFolderTreeQueryHandler(IReadMediaDbContext db)
    : IQueryHandler<GetMediaFolderTreeQuery, Result<MediaFolderTree>>
{
    public async ValueTask<Result<MediaFolderTree>> Handle(GetMediaFolderTreeQuery query,
        CancellationToken cancellationToken)
    {
        var counts = await db.MediaFiles.AsNoTracking()
            .GroupBy(file => file.FolderId)
            .Select(group => new { FolderId = group.Key, Count = group.Count() })
            .ToListAsync(cancellationToken);
        var folders = await db.MediaFolders.AsNoTracking()
            .OrderBy(folder => folder.Name)
            .Select(folder => new { folder.Id, folder.ParentId, folder.Name })
            .ToListAsync(cancellationToken);

        var byFolder = counts.Where(c => c.FolderId != null).ToDictionary(c => c.FolderId!.Value, c => c.Count);
        var items = folders
            .Select(folder => new MediaFolderItem(folder.Id.Value, folder.ParentId?.Value, folder.Name,
                byFolder.GetValueOrDefault(folder.Id)))
            .ToList();
        var root = counts.Where(c => c.FolderId == null).Sum(c => c.Count);
        return Result.Success(new MediaFolderTree(items, root, counts.Sum(c => c.Count)));
    }
}
