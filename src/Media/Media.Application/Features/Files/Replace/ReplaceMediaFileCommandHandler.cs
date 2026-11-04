using Media.Application.Contracts.Images;
using Media.Application.Contracts.Storage;
using Media.Application.Features.Common;
using Media.Application.Features.Files.Common;
using Microsoft.Extensions.Logging;

namespace Media.Application.Features.Files.Replace;

/// <summary>
/// Uploads the new picture, saves the row, then deletes the old CDN file. A failed old-file delete only leaves
/// an orphan on the CDN (logged), never a broken link.
/// </summary>
public sealed class ReplaceMediaFileCommandHandler(
    IMediaRepository<MediaFile> files,
    IImageInspector inspector,
    IMediaStorage storage,
    IMediaUrlBuilder urls,
    TimeProvider clock,
    ILogger<ReplaceMediaFileCommandHandler> logger)
    : ICommandHandler<ReplaceMediaFileCommand, Result<MediaFileItem>>
{
    public async ValueTask<Result<MediaFileItem>> Handle(ReplaceMediaFileCommand command,
        CancellationToken cancellationToken)
    {
        var file = await files.FirstOrDefaultAsync(new MediaFileByIdSpec(MediaFileId.Create(command.Id)), cancellationToken);
        if (file is null)
            return Result.NotFound();

        var now = clock.GetUtcNow();
        var stored = await MediaUploadStore.StoreAsync(command.File, inspector, storage, now, cancellationToken);
        if (!stored.IsSuccess)
            return MediaResults.Fail<MediaFileItem>(stored);

        string previousKey;
        try
        {
            previousKey = file.Replace(stored.Value, now);
            await files.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            await storage.DeleteAsync(stored.Value.StorageKey, CancellationToken.None);
            throw;
        }

        try
        {
            await storage.DeleteAsync(previousKey, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogWarning(exception, "Old CDN file {StorageKey} of media file {MediaFileId} was not deleted",
                previousKey, file.Id.Value);
        }

        return Result.Success(MediaFileMapper.ToItem(file, urls));
    }
}
