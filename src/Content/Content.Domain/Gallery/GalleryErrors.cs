namespace Content.Domain.Gallery;

public static class GalleryErrors
{
    public static DomainError TooLong(string field, int maxLength, int length) =>
        new($"Content.Gallery.{field}.TooLong", $"Не більше {maxLength} символів (зараз {length}).", Field: field);

    public static DomainError MediaFileInvalid(long id) =>
        new("Content.Gallery.MediaFile.Invalid", "Виберіть фото з медіатеки.", Field: "MediaFileId");

    public static DomainError YoutubeLinkInvalid(string? value) =>
        new("Content.Gallery.Youtube.Invalid", "Це не посилання на відео YouTube.", Field: "Youtube");

    public static DomainError PositionInvalid(int position) =>
        new("Content.Gallery.Position.Invalid", $"Позиція не може бути від'ємною (зараз {position}).", Field: "Position");
}
