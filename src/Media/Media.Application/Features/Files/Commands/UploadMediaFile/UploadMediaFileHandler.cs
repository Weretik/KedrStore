using Media.Application.Abstractions.Images;
using Media.Application.Abstractions.Storage;
using Media.Application.Features.Shared;
using Media.Application.Features.Files.Shared;
using Media.Application.Features.Folders.Shared;

namespace Media.Application.Features.Files.Commands.UploadMediaFile;

/// <summary>Puts the picture on the CDN first, then saves the row; a failed save removes the new CDN file.</summary>
public sealed class UploadMediaFileHandler(
    IMediaRepository<MediaFile> files,
    IReadMediaDbContext db,
    IImageInspector inspector,
    IMediaStorage storage,
    IMediaUrlBuilder urls,
    TimeProvider clock)
    : ICommandHandler<UploadMediaFileCommand, Result<MediaFileItem>>
{
    public async ValueTask<Result<MediaFileItem>> Handle(UploadMediaFileCommand command,
        CancellationToken cancellationToken)
    {
        var missing = await MediaFolderRules.CheckExistsAsync(db, command.FolderId, "folderId", cancellationToken);
        if (missing is not null)
            return MediaResults.Fail<MediaFileItem>(missing);

        var now = clock.GetUtcNow();
        var stored = await MediaUploadStore.StoreAsync(command.File, inspector, storage, now, cancellationToken);
        if (!stored.IsSuccess)
            return MediaResults.Fail<MediaFileItem>(stored);

        MediaFolderId? folder = command.FolderId is { } id ? MediaFolderId.Create(id) : null;
        try
        {
            var created = MediaFile.Create(folder, command.File.FileName, stored.Value, now);
            if (created.IsFailure)
            {
                await storage.DeleteAsync(stored.Value.StorageKey, CancellationToken.None);
                return created.Error!.ToResult<MediaFileItem>();
            }

            var file = await files.AddAsync(created.Value, cancellationToken);
            return Result.Success(MediaFileMapper.ToItem(file, urls));
        }
        catch
        {
            await storage.DeleteAsync(stored.Value.StorageKey, CancellationToken.None);
            throw;
        }
    }
}
