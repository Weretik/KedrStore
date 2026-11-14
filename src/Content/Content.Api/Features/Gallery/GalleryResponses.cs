using Content.Application.Features.Gallery.Shared;
using Content.Domain.Gallery;

namespace Content.Api.Features.Gallery;

/// <summary>A site gallery card in one language. <c>kind</c>: <c>photo</c> or <c>video</c>.</summary>
public sealed record PublicGalleryItemResponse(
    long Id,
    string Kind,
    string? Title,
    string? Caption,
    string ImageUrl,
    int? Width,
    int? Height,
    string? YoutubeId);

/// <summary>An admin gallery card; <c>fileMissing</c> marks a photo whose library file was deleted.</summary>
public sealed record AdminGalleryItemResponse(
    long Id,
    string Kind,
    long? MediaFileId,
    string? YoutubeId,
    int Position,
    string? TitleUk,
    string? TitleRu,
    string? CaptionUk,
    string? CaptionRu,
    string? PreviewUrl,
    string? FileName,
    bool FileMissing,
    DateTimeOffset UpdatedAt);

public sealed record YoutubePreviewResponse(string YoutubeId, string? Title, string ThumbnailUrl, bool AlreadyInGallery);

public sealed record AddGalleryPhotosRequest(IReadOnlyList<long>? MediaFileIds);

public sealed record AddGalleryVideoRequest(string? Url, string? TitleUk, string? TitleRu, string? CaptionUk,
    string? CaptionRu);

public sealed record UpdateGalleryItemRequest(string? TitleUk, string? TitleRu, string? CaptionUk, string? CaptionRu);

public sealed record ReorderGalleryRequest(IReadOnlyList<long>? Ids);

internal static class GalleryResponseMapper
{
    public static string Kind(GalleryItemKind kind) => kind == GalleryItemKind.Photo ? "photo" : "video";

    public static PublicGalleryItemResponse ToResponse(PublicGalleryItem item) =>
        new(item.Id, Kind(item.Kind), item.Title, item.Caption, item.ImageUrl, item.Width, item.Height, item.YoutubeId);

    public static AdminGalleryItemResponse ToResponse(AdminGalleryItem item) =>
        new(item.Id, Kind(item.Kind), item.MediaFileId, item.YoutubeId, item.Position, item.TitleUk, item.TitleRu,
            item.CaptionUk, item.CaptionRu, item.PreviewUrl, item.FileName, item.FileMissing, item.UpdatedAt);

    public static IReadOnlyList<AdminGalleryItemResponse> ToResponse(IReadOnlyList<AdminGalleryItem> items) =>
        items.Select(ToResponse).ToList();

    public static YoutubePreviewResponse ToResponse(YoutubePreview preview) =>
        new(preview.YoutubeId, preview.Title, preview.ThumbnailUrl, preview.AlreadyInGallery);
}
