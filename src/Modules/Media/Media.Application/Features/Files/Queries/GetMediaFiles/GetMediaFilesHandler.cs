using BuildingBlocks.Application.Text;
using Media.Application.Abstractions.Storage;
using Media.Application.Features.Files.Shared;
using Media.Application.Features.Folders.Shared;

namespace Media.Application.Features.Files.Queries.GetMediaFiles;

public sealed class GetMediaFilesHandler(IReadMediaDbContext db, IMediaUrlBuilder urls)
    : IQueryHandler<GetMediaFilesQuery, Result<MediaFilePage>>
{
    public async ValueTask<Result<MediaFilePage>> Handle(GetMediaFilesQuery query, CancellationToken cancellationToken)
    {
        var files = db.MediaFiles.AsNoTracking();
        if (query.FolderId is { } folderId)
        {
            var id = MediaFolderId.Create(folderId);
            if (query.IncludeSubfolders)
            {
                var ids = (await MediaFolderRules.SelfAndDescendantsAsync(db, id, cancellationToken))
                    .Select(folder => (MediaFolderId?)folder).ToList();
                files = files.Where(file => ids.Contains(file.FolderId));
            }
            else
            {
                files = files.Where(file => file.FolderId == id);
            }
        }
        else if (query.InRoot)
        {
            files = files.Where(file => file.FolderId == null);
        }

        // Database rules 2.1: every word must match; a trigram index serves ILIKE on the file name.
        foreach (var pattern in SearchWords.Patterns(query.Search))
            files = files.Where(file => EF.Functions.ILike(file.FileName, pattern, SearchWords.EscapeCharacter));

        var total = await files.CountAsync(cancellationToken);
        var page = await files
            .OrderBy(file => file.FileName).ThenBy(file => file.Id)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return Result.Success(new MediaFilePage(
            page.Select(file => MediaFileMapper.ToItem(file, urls)).ToList(), query.Page, query.PageSize, total));
    }
}
