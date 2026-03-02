using System.Globalization;

namespace Catalog.Domain.Errors;

public static class ProductErrors
{
    public static CatalogDomainError IdRequired() =>
        new("Catalog.Product.Id.Required",
            "Product id is required");

    public static CatalogDomainError NameRequired() =>
        new("Catalog.Product.Name.Required",
            "Product name is required");

    public static CatalogDomainError NameLengthInvalid(int length) =>
        new("Catalog.Product.Name.LengthInvalid",
            $"Product name length must be between 1 and 300 characters. Actual: {length}");

    public static CatalogDomainError CategoryIdRequired() =>
        new("Catalog.Product.CategoryId.Required",
            "Product category id is required");

    public static CatalogDomainError PhotoRequired() =>
        new("Catalog.Product.Photo.Required",
            "Product photo is required");

    public static CatalogDomainError SchemeInvalid() =>
        new("Catalog.Product.Scheme.Invalid",
            "Product scheme cannot be empty or whitespace");

    public static CatalogDomainError StockOutOfRange(decimal value) =>
        new("Catalog.Product.Stock.OutOfRange",
            $"Stock must be between 0 and 10000. Actual: {value}");

    public static CatalogDomainError QuantityInPackNegative(int value) =>
        new("Catalog.Product.QuantityInPack.Negative",
            $"Quantity in pack cannot be negative. Actual: {value}");
}
