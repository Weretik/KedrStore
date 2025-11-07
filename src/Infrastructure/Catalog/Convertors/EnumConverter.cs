using Domain.Catalog.Enumerations;

namespace Infrastructure.Catalog.Convertors;

public static class EnumConverter
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

}
