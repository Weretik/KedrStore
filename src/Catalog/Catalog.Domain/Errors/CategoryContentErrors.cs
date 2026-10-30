namespace Catalog.Domain.Errors;

public static class CategoryContentErrors
{
    public static CatalogDomainError CategoryIdRequired() =>
        new("Catalog.CategoryContent.CategoryId.Required", "Category id must be greater than zero");

    public static CatalogDomainError LanguageInvalid(string? language) =>
        new("Catalog.CategoryContent.Language.Invalid", $"Content language must be 'uk' or 'ru'. Actual: {language}");

    public static CatalogDomainError Required(string field) =>
        new($"Catalog.CategoryContent.{field}.Required", $"{field} is required");

    public static CatalogDomainError TooLong(string field, int maxLength, int length) =>
        new($"Catalog.CategoryContent.{field}.TooLong",
            $"{field} must be at most {maxLength} characters. Actual: {length}");

    public static CatalogDomainError TooManyFaqItems(int count) =>
        new("Catalog.CategoryContent.Faq.TooMany",
            $"A category can have at most {Entities.CategoryContent.FaqMaxItems} FAQ items. Actual: {count}");
}
