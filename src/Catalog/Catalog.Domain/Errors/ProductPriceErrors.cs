// Licensed to KedrStore Development Team under MIT License.

namespace Catalog.Domain.Errors;

public static class ProductPriceErrors
{
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
            "Price amount must be between 0 and 100000",
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

    public static DomainError PriceTypeIdOutOfRange(int value) =>
        new(
            "Catalog.ProductPrice.PriceType.Id.OutOfRange",
            "PriceType id must be between 1 and 14",
            new { value, min = 1, max = 14 }
        );

    public static DomainError PriceTypeNameRequired() =>
        new("Catalog.ProductPrice.PriceType.Name.Required", "PriceType name is required");

    public static DomainError UnknownPriceTypeName(string name) =>
        new(
            "Catalog.ProductPrice.PriceType.Name.Unknown",
            "Unknown PriceType name",
            new { name }
        );
}
