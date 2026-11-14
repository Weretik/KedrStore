namespace Work.Domain;

public static class WorkErrors
{
    public static DomainError Required(string field) =>
        new($"Work.{field}.Required", "Заповніть це поле.", Field: field);

    public static DomainError TooLong(string field, int maxLength, int length) =>
        new($"Work.{field}.TooLong", $"Не більше {maxLength} символів (зараз {length}).", Field: field);

    public static DomainError TooManySteps(int maxCount, int count) =>
        new("Work.Steps.TooMany", $"Не більше {maxCount} кроків (зараз {count}).", Field: "Steps");

    public static DomainError UserRequired() =>
        new("Work.User.Required", "Задача й запис журналу потребують користувача.");
}
