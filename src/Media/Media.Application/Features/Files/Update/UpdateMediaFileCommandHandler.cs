using Media.Application.Contracts.Storage;
using Media.Application.Features.Common;
using Media.Application.Features.Files.Common;
using Media.Application.Features.Folders.Common;

namespace Media.Application.Features.Files.Update;

public sealed class UpdateMediaFileCommandHandler(
    IMediaRepository<MediaFile> files,
    IReadMediaDbContext db,
    IMediaUrlBuilder urls,
    TimeProvider clock)
    : ICommandHandler<UpdateMediaFileCommand, Result<MediaFileItem>>
{
    public async ValueTask<Result<MediaFileItem>> Handle(UpdateMediaFileCommand command,
        CancellationToken cancellationToken)
    {
        var file = await files.FirstOrDefaultAsync(new MediaFileByIdSpec(MediaFileId.Create(command.Id)), cancellationToken);
        if (file is null)
            return Result.NotFound();

        var missing = await MediaFolderRules.CheckExistsAsync(db, command.FolderId, "folderId", cancellationToken);
        if (missing is not null)
            return MediaResults.Fail<MediaFileItem>(missing);

        var now = clock.GetUtcNow();
        file.Rename(command.FileName, now);
        file.MoveTo(command.FolderId is { } id ? MediaFolderId.Create(id) : null, now);
        await files.SaveChangesAsync(cancellationToken);
        return Result.Success(MediaFileMapper.ToItem(file, urls));
    }
}
