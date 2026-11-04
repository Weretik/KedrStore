using Media.Application.Contracts.Storage;

namespace Media.Application.Features.Files.Common;

internal static class MediaFileMapper
{
    public static MediaFileItem ToItem(MediaFile file, IMediaUrlBuilder urls) => new(
        file.Id.Value, file.FolderId?.Value, file.FileName, urls.Build(file.StorageKey), file.ContentType,
        file.SizeBytes, file.Width, file.Height, file.CreatedAt, file.UpdatedAt);
}
