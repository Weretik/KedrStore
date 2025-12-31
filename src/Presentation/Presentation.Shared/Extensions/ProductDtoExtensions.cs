using Catalog.Application.Shared;

namespace Presentation.Shared.Extensions;

public static class ProductDtoExtensions
{
    public static decimal GetPriceForType(this ProductDto product, string priceType)
    {
        return product.Prices
            .FirstOrDefault(productPriceDto => productPriceDto.PriceType == priceType)?.Amount ?? 0m;
    }

    public static string GetCurrencyForType(this ProductDto product, string priceType)
    {
        return product.Prices
            .FirstOrDefault(productPriceDto => productPriceDto.PriceType == priceType)?.CurrencyIso ?? "";
    }
}
