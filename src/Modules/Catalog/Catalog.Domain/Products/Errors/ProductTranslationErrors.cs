using BuildingBlocks.Domain.Abstractions.Errors;

namespace Catalog.Domain.Products.Errors;

public static class ProductTranslationErrors
{
    public static DomainError ProductIdRequired() =>
        new("Catalog.ProductTranslation.ProductId.Required", "Потрібен код товару.", Field: "ProductId");

    public static DomainError LanguageRequired() =>
        new("Catalog.ProductTranslation.Language.Required", "Вкажіть мову.", Field: "Language");

    public static DomainError LanguageInvalid() =>
        new("Catalog.ProductTranslation.Language.LengthInvalid", "Код мови має містити від 2 до 10 символів.",
            Field: "Language");

    public static DomainError NameRequired() =>
        new("Catalog.ProductTranslation.Name.Required", "Заповніть це поле.", Field: "Name");

    public static DomainError NameTooLong(int length) =>
        new("Catalog.ProductTranslation.Name.LengthInvalid",
            $"Не більше {ProductTranslation.NameMaxLength} символів (зараз {length}).", Field: "Name");
}
