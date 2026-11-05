namespace Content.Domain.Wholesale;

public static class WholesaleErrors
{
    public static DomainError Required(string field) =>
        new($"Content.Wholesale.{field}.Required", $"{field} is required");

    public static DomainError TooLong(string field, int maxLength, int length) =>
        new($"Content.Wholesale.{field}.TooLong", $"{field} must be at most {maxLength} characters. Actual: {length}");

    public static DomainError StepCount(int expected, int actual) =>
        new("Content.Wholesale.Steps.Count", $"Exactly {expected} steps are required. Actual: {actual}");

    public static DomainError FaqCount(int max, int actual) =>
        new("Content.Wholesale.Faq.Count", $"From 1 to {max} questions are required. Actual: {actual}");

    public static DomainError UnknownLanguage(string language) =>
        new("Content.Wholesale.Language.Unknown", $"Unknown language: {language}");
}
