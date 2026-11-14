namespace Catalog.Domain.Categories.Content;

public static class CategoryContentErrors
{
    public static DomainError CategoryIdRequired() =>
        new("Catalog.CategoryContent.CategoryId.Required", "Category id must be greater than zero");

    public static DomainError LanguageInvalid(string? language) =>
        new("Catalog.CategoryContent.Language.Invalid", $"Content language must be 'uk' or 'ru'. Actual: {language}");

    public static DomainError Required(string field) =>
        new($"Catalog.CategoryContent.{field}.Required", $"{field} is required");

    public static DomainError TooLong(string field, int maxLength, int length) =>
        new($"Catalog.CategoryContent.{field}.TooLong",
            $"{field} must be at most {maxLength} characters. Actual: {length}");

    public static DomainError TooManyFaqItems(int count) =>
        new("Catalog.CategoryContent.Faq.TooMany",
            $"A category can have at most {CategoryContent.FaqMaxItems} FAQ items. Actual: {count}");
}
