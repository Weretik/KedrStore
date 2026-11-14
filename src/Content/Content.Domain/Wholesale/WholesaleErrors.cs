namespace Content.Domain.Wholesale;

public static class WholesaleErrors
{
    public static DomainError Required(string field) =>
        new($"Content.Wholesale.{field}.Required", "Заповніть це поле.", Field: field);

    public static DomainError TooLong(string field, int maxLength, int length) =>
        new($"Content.Wholesale.{field}.TooLong", $"Не більше {maxLength} символів (зараз {length}).", Field: field);

    public static DomainError StepCount(int expected, int actual) =>
        new("Content.Wholesale.Steps.Count", $"Потрібно рівно {expected} кроки (зараз {actual}).", Field: "Steps");

    public static DomainError FaqCount(int max, int actual) =>
        new("Content.Wholesale.Faq.Count", $"Потрібно від 1 до {max} питань (зараз {actual}).", Field: "Faq");
}
