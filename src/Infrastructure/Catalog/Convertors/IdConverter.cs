using Domain.Catalog.ValueObjects;

namespace Infrastructure.Catalog.Convertors;

public static class IdConverter
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
}
