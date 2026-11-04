namespace Content.Application.Features.Gallery.Common;

/// <summary>Title and caption of a card in both languages, as typed by the manager.</summary>
public sealed record GalleryTextsInput(string? TitleUk, string? TitleRu, string? CaptionUk, string? CaptionRu);

/// <summary>A gallery card for the admin: its source, texts and a preview picture.</summary>
public sealed record AdminGalleryItem(
    long Id,
    GalleryItemKind Kind,
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

/// <summary>A gallery card for the site in one language.</summary>
public sealed record PublicGalleryItem(
    long Id,
    GalleryItemKind Kind,
    string? Title,
    string? Caption,
    string ImageUrl,
    int? Width,
    int? Height,
    string? YoutubeId);

/// <summary>What a pasted link resolves to before it is added.</summary>
public sealed record YoutubePreview(string YoutubeId, string? Title, string ThumbnailUrl, bool AlreadyInGallery);
