namespace Media.Domain;

public static class MediaErrors
{
    public static DomainError Required(string field) =>
        new($"Media.{field}.Required", "Заповніть це поле.", Field: field);

    public static DomainError TooLong(string field, int maxLength, int length) =>
        new($"Media.{field}.TooLong", $"Не більше {maxLength} символів (зараз {length}).", Field: field);

    public static DomainError FolderMoveIntoItself() =>
        new("Media.Folder.MoveIntoItself", "Папку не можна перенести в неї саму або в її підпапку.", Field: "ParentId");

    public static DomainError ContentInvalid() =>
        new("Media.File.Content.Invalid", "Розмір файлу не може бути від'ємним, а розміри зображення мають бути додатними.");
}
