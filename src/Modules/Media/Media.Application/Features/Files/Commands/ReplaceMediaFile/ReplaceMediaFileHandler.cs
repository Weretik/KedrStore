using Media.Application.Abstractions.Images;
using Media.Application.Abstractions.Storage;
using Media.Application.Features.Shared;
using Media.Application.Features.Files.Shared;
using Microsoft.Extensions.Logging;

namespace Media.Application.Features.Files.Commands.ReplaceMediaFile;

/// <summary>
/// Uploads the new picture, saves the row, then deletes the old CDN file and tells the linking modules
/// about the new URL (R-35). A failed old-file delete only leaves
/// an orphan on the CDN (logged), never a broken link.
/// </summary>
public sealed class ReplaceMediaFileHandler(
    IMediaRepository<MediaFile> files,
    IImageInspector inspector,
    IMediaStorage storage,
    IMediaUrlBuilder urls,
    MediaFileChangeNotifier changes,
    TimeProvider clock,
    ILogger<ReplaceMediaFileHandler> logger)
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
            var replaced = file.Replace(stored.Value, now);
            if (replaced.IsFailure)
            {
                await storage.DeleteAsync(stored.Value.StorageKey, CancellationToken.None);
                return replaced.Error!.ToResult<MediaFileItem>();
            }

            previousKey = replaced.Value;
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

        var item = MediaFileMapper.ToItem(file, urls);
        await changes.ReplacedAsync(file.Id.Value, item.Url, cancellationToken);
        return Result.Success(item);
    }
}
