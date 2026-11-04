using Media.Application.Contracts.Storage;
using Media.Application.Features.Files.Common;

namespace Media.Application.Features.Files.GetById;

public sealed class GetMediaFileQueryHandler(IReadMediaDbContext db, IMediaUrlBuilder urls)
    : IQueryHandler<GetMediaFileQuery, Result<MediaFileItem>>
{
    public async ValueTask<Result<MediaFileItem>> Handle(GetMediaFileQuery query, CancellationToken cancellationToken)
    {
        var id = MediaFileId.Create(query.Id);
        var file = await db.MediaFiles.AsNoTracking().FirstOrDefaultAsync(file => file.Id == id, cancellationToken);
        return file is null ? Result.NotFound() : Result.Success(MediaFileMapper.ToItem(file, urls));
    }
}
