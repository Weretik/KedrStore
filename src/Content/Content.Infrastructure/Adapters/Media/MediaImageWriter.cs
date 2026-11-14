using Content.Application.Abstractions.Media;
using Media.Contracts.Files;

namespace Content.Infrastructure.Adapters.Media;

/// <summary>Article covers go into the library through the Media module's published uploader, never its tables.</summary>
internal sealed class MediaImageWriter(IMediaFileUploader uploader) : IMediaImageWriter
{
    public const string StorageUnavailableMessage = "Media storage is not configured.";
    public const string NotAPictureMessage = "The file must be a JPEG, PNG, or WebP picture.";

    public async Task<Result<StoredMediaImage>> UploadAsync(MediaImageUpload upload, CancellationToken cancellationToken)
    {
        var result = await uploader.UploadAsync(
            new MediaFileUploadRequest(upload.StorageDirectory, upload.FolderName, upload.FileName, upload.Data),
            cancellationToken);
        return result switch
        {
            { Status: MediaFileUploadStatus.Uploaded, File: { } file } => Result.Success(new StoredMediaImage(file.Id, file.Url)),
            { Status: MediaFileUploadStatus.StorageUnavailable } => Result<StoredMediaImage>.Unavailable(StorageUnavailableMessage),
            _ => Result<StoredMediaImage>.Invalid(new ValidationError("file", NotAPictureMessage)),
        };
    }
}
