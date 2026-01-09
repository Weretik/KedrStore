using Catalog.Domain.ValueObjects;

namespace Catalog.Infrastructure.Configurations;

public static class CatalogConverter
{
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
    public static readonly ValueConverter<ProductPriceId, int> ProductPriceIdConvert =
        new(
            id => id.Value,
            value => ProductPriceId.From(value)
        );
    public static readonly ValueConverter<PriceTypeId, int> PriceTypeIdConvert =
        new(
            id => id.Value,
            value => PriceTypeId.From(value)
        );

}
