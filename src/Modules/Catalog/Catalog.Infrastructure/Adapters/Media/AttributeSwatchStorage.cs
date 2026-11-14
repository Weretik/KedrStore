using Catalog.Application.Abstractions.Media;
using Media.Contracts.Files;

namespace Catalog.Infrastructure.Adapters.Media;

/// <summary>Swatch pictures go into the library through the Media module's published uploader, never its tables.</summary>
internal sealed class AttributeSwatchStorage(IMediaFileUploader uploader) : IAttributeSwatchStorage
{
    public const string StorageDirectory = "swatches";
    public const string LibraryFolder = "Кольори";

    public async Task<Result<StoredSwatchImage>> UploadAsync(string fileName, byte[] data,
        CancellationToken cancellationToken)
    {
        var result = await uploader.UploadAsync(
            new MediaFileUploadRequest(StorageDirectory, LibraryFolder, fileName, data), cancellationToken);
        return result switch
        {
            { Status: MediaFileUploadStatus.Uploaded, File: { } file } =>
                Result.Success(new StoredSwatchImage(file.Id, file.Url)),
            { Status: MediaFileUploadStatus.StorageUnavailable } =>
                Result<StoredSwatchImage>.Unavailable("Сховище медіа не налаштоване."),
            { Status: MediaFileUploadStatus.Invalid, Message: { } message } =>
                Result<StoredSwatchImage>.Invalid(new ValidationError("file", message, null, ValidationSeverity.Error)),
            _ => Result<StoredSwatchImage>.Invalid(
                new ValidationError("file", "Файл має бути зображенням JPEG, PNG або WebP.", null,
                    ValidationSeverity.Error)),
        };
    }
}
