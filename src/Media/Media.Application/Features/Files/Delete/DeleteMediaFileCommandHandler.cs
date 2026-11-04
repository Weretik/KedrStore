using Media.Application.Contracts.Storage;
using Media.Application.Features.Files.Common;

namespace Media.Application.Features.Files.Delete;

/// <summary>Deletes the CDN file first, so a failure leaves the row and the admin can retry.</summary>
public sealed class DeleteMediaFileCommandHandler(IMediaRepository<MediaFile> files, IMediaStorage storage)
    : ICommandHandler<DeleteMediaFileCommand, Result>
{
    public async ValueTask<Result> Handle(DeleteMediaFileCommand command, CancellationToken cancellationToken)
    {
        var file = await files.FirstOrDefaultAsync(new MediaFileByIdSpec(MediaFileId.Create(command.Id)), cancellationToken);
        if (file is null)
            return Result.NotFound();
        if (!storage.IsConfigured)
            return Result.Unavailable(MediaUploadStore.StorageUnavailableMessage);

        await storage.DeleteAsync(file.StorageKey, cancellationToken);
        await files.DeleteAsync(file, cancellationToken);
        return Result.Success();
    }
}
