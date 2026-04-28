namespace Catalog.Domain.Errors;

public static class ProductTranslationErrors
{
    public static CatalogDomainError ProductIdRequired() =>
        new("Catalog.ProductTranslation.ProductId.Required",
            "Product id is required");

    public static CatalogDomainError LanguageRequired() =>
        new("Catalog.ProductTranslation.Language.Required",
            "Language is required");

    public static CatalogDomainError LanguageLengthInvalid(int length) =>
        new("Catalog.ProductTranslation.Language.LengthInvalid",
            $"Language length must be between 2 and 10 characters. Actual: {length}");

    public static CatalogDomainError NameRequired() =>
        new("Catalog.ProductTranslation.Name.Required",
            "Translated product name is required");

    public static CatalogDomainError NameLengthInvalid(int length) =>
        new("Catalog.ProductTranslation.Name.LengthInvalid",
            $"Translated product name length must be between 1 and 300 characters. Actual: {length}");
}
