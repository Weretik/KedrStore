using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;

namespace Catalog.Infrastructure.Converters;

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
    public static readonly ValueConverter<PriceTypeId, int> PriceTypeIdConvert =
        new(
            id => id.Value,
            value => PriceTypeId.From(value)
        );

}
