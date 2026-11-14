using Media.Application.Abstractions.Storage;

namespace Media.Application.Features.Files.Shared;

internal static class MediaFileMapper
{
    public static MediaFileItem ToItem(MediaFile file, IMediaUrlBuilder urls) => new(
        file.Id.Value, file.FolderId?.Value, file.FileName, urls.Build(file.StorageKey), file.ContentType,
        file.SizeBytes, file.Width, file.Height, file.CreatedAt, file.UpdatedAt);
}
