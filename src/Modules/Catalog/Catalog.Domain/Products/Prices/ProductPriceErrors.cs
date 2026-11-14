namespace Catalog.Domain.Products.Prices;

public static class ProductPriceErrors
{
    public static DomainError ProductIdRequired() =>
        new("Catalog.ProductPrice.ProductId.Required", "Потрібен код товару.", Field: "ProductId");

    public static DomainError PriceTypeRequired() =>
        new("Catalog.ProductPrice.PriceType.Required", "Оберіть тип ціни.", Field: "PriceTypeId");

    public static DomainError AmountNegative() =>
        new("Catalog.ProductPrice.Amount.Negative", "Ціна не може бути від'ємною.", Field: "Amount");

    public static DomainError AmountOutOfRange() =>
        new("Catalog.ProductPrice.Amount.OutOfRange", $"Ціна має бути від 0 до {ProductPrice.MaximumAmount:0}.",
            Field: "Amount");

    public static DomainError CurrencyCodeRequired() =>
        new("Catalog.ProductPrice.Currency.Code.Required", "Вкажіть валюту.", Field: "Currency");

    public static DomainError CurrencyCodeInvalid() =>
        new("Catalog.ProductPrice.Currency.Code.Invalid", "Код валюти: три літери ISO, наприклад UAH.",
            Field: "Currency");
}
