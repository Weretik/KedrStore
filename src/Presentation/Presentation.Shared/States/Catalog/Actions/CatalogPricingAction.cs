using Application.Catalog.GetProducts;

namespace Presentation.Shared.States.Catalog;

public static class CatalogPricingAction
{
    public sealed record SetPricingOptions(PricingOptions Pricing);
    public sealed record SetPriceId(string PriceType);
    public sealed record SetPriceRange(decimal? MinPrice, decimal? MaxPrice);
}
