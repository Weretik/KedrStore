namespace Content.Domain.Home;

public static class HomeErrors
{
    public static DomainError Required(string field) =>
        new($"Content.Home.{field}.Required", "Заповніть це поле.", Field: field);

    public static DomainError TooLong(string field, int maxLength, int length) =>
        new($"Content.Home.{field}.TooLong", $"Не більше {maxLength} символів (зараз {length}).", Field: field);

    public static DomainError MediaFileInvalid(long id) =>
        new("Content.Home.Logo.Invalid", "Виберіть логотип з медіатеки.", Field: "LogoMediaFileId");

    public static DomainError PositionInvalid(int position) =>
        new("Content.Home.Position.Invalid", $"Позиція не може бути від'ємною (зараз {position}).", Field: "Position");
}
