namespace Content.Domain.Gallery;

public static class GalleryErrors
{
    public static DomainError TooLong(string field, int maxLength, int length) =>
        new($"Content.Gallery.{field}.TooLong", $"{field} must be at most {maxLength} characters. Actual: {length}");

    public static DomainError MediaFileInvalid(long id) =>
        new("Content.Gallery.MediaFile.Invalid", $"A media file ID must be positive. Actual: {id}");

    public static DomainError YoutubeLinkInvalid(string? value) =>
        new("Content.Gallery.Youtube.Invalid", $"Not a YouTube video link: {value}");

    public static DomainError PositionInvalid(int position) =>
        new("Content.Gallery.Position.Invalid", $"A position must not be negative. Actual: {position}");
}
