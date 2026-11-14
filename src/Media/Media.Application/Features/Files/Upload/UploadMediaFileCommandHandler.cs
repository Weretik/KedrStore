using Media.Application.Contracts.Images;
using Media.Application.Contracts.Storage;
using Media.Application.Features.Common;
using Media.Application.Features.Files.Common;
using Media.Application.Features.Folders.Common;

namespace Media.Application.Features.Files.Upload;

/// <summary>Puts the picture on the CDN first, then saves the row; a failed save removes the new CDN file.</summary>
public sealed class UploadMediaFileCommandHandler(
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
