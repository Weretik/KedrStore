namespace Media.Domain;

public static class MediaErrors
{
    public static DomainError Required(string field) =>
        new($"Media.{field}.Required", $"{field} is required");

    public static DomainError TooLong(string field, int maxLength, int length) =>
        new($"Media.{field}.TooLong", $"{field} must be at most {maxLength} characters. Actual: {length}");

    public static DomainError FolderMoveIntoItself() =>
        new("Media.Folder.MoveIntoItself", "A folder cannot be moved into itself or into its subfolder");

    public static DomainError ContentInvalid() =>
        new("Media.File.Content.Invalid", "File size must not be negative and picture dimensions must be positive");
}
