namespace Catalog.Domain.Products;

public static class ProductTranslationErrors
{
    public static DomainError ProductIdRequired() =>
        new("Catalog.ProductTranslation.ProductId.Required",
            "Product id is required");

    public static DomainError LanguageRequired() =>
        new("Catalog.ProductTranslation.Language.Required",
            "Language is required");

    public static DomainError LanguageLengthInvalid(int length) =>
        new("Catalog.ProductTranslation.Language.LengthInvalid",
            $"Language length must be between 2 and 10 characters. Actual: {length}");

    public static DomainError NameRequired() =>
        new("Catalog.ProductTranslation.Name.Required",
            "Translated product name is required");

    public static DomainError NameLengthInvalid(int length) =>
        new("Catalog.ProductTranslation.Name.LengthInvalid",
            $"Translated product name length must be between 1 and 300 characters. Actual: {length}");
}
