namespace Content.Domain.Home;

public static class HomeErrors
{
    public static DomainError Required(string field) =>
        new($"Content.Home.{field}.Required", $"{field} is required.");

    public static DomainError TooLong(string field, int maxLength, int length) =>
        new($"Content.Home.{field}.TooLong", $"{field} must be at most {maxLength} characters. Actual: {length}");

    public static DomainError MediaFileInvalid(long id) =>
        new("Content.Home.Logo.Invalid", $"A media file ID must be positive. Actual: {id}");

    public static DomainError PositionInvalid(int position) =>
        new("Content.Home.Position.Invalid", $"A position must not be negative. Actual: {position}");
}
