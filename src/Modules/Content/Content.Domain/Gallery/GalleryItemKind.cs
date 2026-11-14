namespace Content.Domain.Gallery;

public enum GalleryItemKind
{
    /// <summary>A picture from the media library, linked by its file ID.</summary>
    Photo = 1,

    /// <summary>A YouTube video, stored by its 11-character video ID.</summary>
    Video = 2,
}
