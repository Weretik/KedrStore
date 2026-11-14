using BuildingBlocks.Domain.Abstractions.Errors;

namespace Catalog.Domain.Products.Errors;

public static class ProductErrors
{
    public static DomainError IdRequired() =>
        new("Catalog.Product.Id.Required", "Потрібен код товару.", Field: "Id");

    public static DomainError NameRequired() =>
        new("Catalog.Product.Name.Required", "Заповніть це поле.", Field: "Name");

    public static DomainError NameTooLong(int length) =>
        new("Catalog.Product.Name.LengthInvalid", $"Не більше {Product.NameMaxLength} символів (зараз {length}).",
            Field: "Name");

    public static DomainError SlugRequired() =>
        new("Catalog.Product.Slug.Required", "Заповніть це поле.", Field: "Slug");

    public static DomainError CategoryIdRequired() =>
        new("Catalog.Product.CategoryId.Required", "Оберіть категорію.", Field: "CategoryId");

    public static DomainError StockOutOfRange(decimal value) =>
        new("Catalog.Product.Stock.OutOfRange", $"Залишок має бути від 0 до {Product.MaximumStock:0} (зараз {value}).",
            Field: "Stock");

    public static DomainError QuantityInPackNegative() =>
        new("Catalog.Product.QuantityInPack.Negative", "Кількість в упаковці не може бути від'ємною.",
            Field: "QuantityInPack");
}
