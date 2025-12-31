using Catalog.Domain.Enumerations;
using Catalog.Domain.ValueObjects;

namespace Catalog.Infrastructure.Configurations;

public static class CatalogConverter
{
    public static readonly ValueConverter<ProductType, string> ProductTypeConvert =
        new(
            productType => productType.Name,
            name => ProductType.FromName(name, false)
        );

    public static readonly ValueConverter<PriceType, string> PriceTypeConvert =
        new(
            priceType => priceType.Name,
            name => PriceType.FromName(name, false)
        );

    public static readonly ValueConverter<ProductId, int> ProductIdConvert =
        new(
            id => id.Value,
            value => ProductId.From(value)
        );

    public static readonly ValueConverter<ProductCategoryId, int> ProductCategoryIdConvert =
        new(
            id => id.Value,
            value => ProductCategoryId.From(value)
        );
}
