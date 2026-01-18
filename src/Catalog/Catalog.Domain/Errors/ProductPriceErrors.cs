namespace Catalog.Domain.Errors;

public static class ProductPriceErrors
{
    public static DomainError IdRequired() =>
        new("Catalog.ProductPrice.Id.Required", "Product price id is required");

    public static DomainError ProductIdRequired() =>
        new("Catalog.ProductId.PriceType.Required", "Product id type is required");
    public static DomainError PriceTypeRequired() =>
        new("Catalog.ProductPrice.PriceType.Required", "Price type is required");

    public static DomainError AmountNegative(decimal value) =>
        new(
            "Catalog.ProductPrice.Amount.Negative",
            "Price amount cannot be negative",
            new { value }
        );

    public static DomainError AmountOutOfRange(decimal value) =>
        new(
            "Catalog.ProductPrice.Amount.OutOfRange",
            "Price amount must be between 0 and 100000 Actual",
            new { value, min = 0.00m, max = 100_000m }
        );

    public static DomainError CurrencyCodeRequired() =>
        new("Catalog.ProductPrice.Currency.Code.Required", "Currency ISO code is required");

    public static DomainError CurrencyCodeLengthInvalid(string code) =>
        new(
            "Catalog.ProductPrice.Currency.Code.LengthInvalid",
            "Currency ISO code must be exactly 3 characters",
            new { code }
        );

    public static DomainError CurrencyCodeNonLetters(string code) =>
        new(
            "Catalog.ProductPrice.Currency.Code.NonLetters",
            "Currency ISO code must contain only letters",
            new { code }
        );

}
