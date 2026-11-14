namespace Content.Domain.Gallery;

/// <summary>
/// One card of the site gallery: a media-library photo (by file ID, no cross-module key) or a YouTube video,
/// with optional texts in both languages. The site shows items by <see cref="Position"/>, smallest first.
/// </summary>
public sealed class GalleryItem : IAggregateRoot
{
    public long Id { get; private set; }
    public GalleryItemKind Kind { get; private set; }

    /// <summary>Media library file ID; set only for a photo.</summary>
    public long? MediaFileId { get; private set; }

    /// <summary>YouTube video ID; set only for a video.</summary>
    public string? YoutubeId { get; private set; }

    public int Position { get; private set; }
    public string? TitleUk { get; private set; }
    public string? TitleRu { get; private set; }
    public string? CaptionUk { get; private set; }
    public string? CaptionRu { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private GalleryItem() { }

    public static DomainResult<GalleryItem> CreatePhoto(long mediaFileId, int position, GalleryItemTexts texts,
        DateTimeOffset now)
    {
        if (mediaFileId <= 0)
            return GalleryErrors.MediaFileInvalid(mediaFileId);
        return Create(GalleryItemKind.Photo, mediaFileId, null, position, texts, now);
    }

    public static DomainResult<GalleryItem> CreateVideo(string youtubeLinkOrId, int position, GalleryItemTexts texts,
        DateTimeOffset now)
    {
        var youtubeId = YoutubeVideoId.Parse(youtubeLinkOrId);
        if (youtubeId.IsFailure)
            return youtubeId.Error!;
        return Create(GalleryItemKind.Video, null, youtubeId.Value, position, texts, now);
    }

    public void UpdateTexts(GalleryItemTexts texts, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(texts);
        TitleUk = texts.TitleUk;
        TitleRu = texts.TitleRu;
        CaptionUk = texts.CaptionUk;
        CaptionRu = texts.CaptionRu;
        UpdatedAt = now;
    }

    public DomainResult MoveTo(int position, DateTimeOffset now)
    {
        if (position < 0)
            return GalleryErrors.PositionInvalid(position);
        if (position == Position)
            return DomainResult.Success();
        Position = position;
        UpdatedAt = now;
        return DomainResult.Success();
    }

    private static DomainResult<GalleryItem> Create(GalleryItemKind kind, long? mediaFileId, string? youtubeId,
        int position, GalleryItemTexts texts, DateTimeOffset now)
    {
        if (position < 0)
            return GalleryErrors.PositionInvalid(position);
        var item = new GalleryItem
        {
            Kind = kind,
            MediaFileId = mediaFileId,
            YoutubeId = youtubeId,
            Position = position,
            CreatedAt = now,
        };
        item.UpdateTexts(texts, now);
        return item;
    }
}
