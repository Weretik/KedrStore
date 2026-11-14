using Media.Application.Abstractions.Storage;
using Media.Application.Features.Shared;
using Media.Application.Features.Files.Shared;
using Media.Application.Features.Folders.Shared;

namespace Media.Application.Features.Files.Commands.UpdateMediaFile;

public sealed class UpdateMediaFileHandler(
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
        var renamed = file.Rename(command.FileName, now);
        if (renamed.IsFailure)
            return renamed.Error!.ToResult<MediaFileItem>();
        file.MoveTo(command.FolderId is { } id ? MediaFolderId.Create(id) : null, now);
        await files.SaveChangesAsync(cancellationToken);
        return Result.Success(MediaFileMapper.ToItem(file, urls));
    }
}
