using Media.Application.Contracts.Storage;
using Media.Application.Features.Files.Common;

namespace Media.Application.Features.Files.GetList;

public sealed class GetMediaFilesQueryHandler(IReadMediaDbContext db, IMediaUrlBuilder urls)
    : IQueryHandler<GetMediaFilesQuery, Result<MediaFilePage>>
{
    public async ValueTask<Result<MediaFilePage>> Handle(GetMediaFilesQuery query, CancellationToken cancellationToken)
    {
        var files = db.MediaFiles.AsNoTracking();
        if (query.FolderId is { } folderId)
        {
            var id = MediaFolderId.Create(folderId);
            files = files.Where(file => file.FolderId == id);
        }
        else if (query.InRoot)
        {
            files = files.Where(file => file.FolderId == null);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim().ToLower();
            files = files.Where(file => file.FileName.ToLower().Contains(search));
        }

        var total = await files.CountAsync(cancellationToken);
        var page = await files
            .OrderBy(file => file.FileName).ThenBy(file => file.Id)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return Result.Success(new MediaFilePage(
            page.Select(file => MediaFileMapper.ToItem(file, urls)).ToList(), query.Page, query.PageSize, total));
    }
}
