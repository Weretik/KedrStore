using Media.Application.Abstractions.Images;
using Media.Application.Abstractions.Storage;

namespace Media.Application.Features.Files.Shared;

/// <summary>Checks an upload is a real picture and puts it on the CDN under a new unique key (R-4, R-5).</summary>
internal static class MediaUploadStore
{
    public const string StorageUnavailableMessage = "Сховище медіа не налаштоване.";
    public const string NotAPictureMessage = "Файл має бути зображенням JPEG, PNG або WebP.";
    public const string LibraryDirectory = "library";

    public static async Task<Result<MediaFileContent>> StoreAsync(MediaUpload upload, IImageInspector inspector,
        IMediaStorage storage, DateTimeOffset now, CancellationToken cancellationToken)
    {
        if (!storage.IsConfigured)
            return Result<MediaFileContent>.Unavailable(StorageUnavailableMessage);

        using var buffer = new MemoryStream();
        await using (var source = upload.OpenReadStream())
            await source.CopyToAsync(buffer, cancellationToken);

        buffer.Position = 0;
        var image = inspector.Inspect(buffer);
        if (image is null)
            return Result<MediaFileContent>.Invalid(new ValidationError("file", NotAPictureMessage));

        var key = $"{LibraryDirectory}/{now:yyyy}/{now:MM}/{Guid.NewGuid():N}{image.Extension}";
        buffer.Position = 0;
        await storage.UploadAsync(key, buffer, cancellationToken);
        return Result.Success(new MediaFileContent(key, image.ContentType, buffer.Length, image.Width, image.Height));
    }
}
