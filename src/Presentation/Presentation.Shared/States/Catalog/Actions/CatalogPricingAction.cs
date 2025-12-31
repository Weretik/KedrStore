using Catalog.Application.Features.Products.Queries.GetProducts;
using Catalog.Application.GetProducts;

namespace Presentation.Shared.States.Catalog;

public static class CatalogPricingAction
{
    public sealed record SetPricingOptions(PricingOptions Pricing);
    public sealed record SetPriceType(string PriceType);
    public sealed record SetPriceRange(decimal? MinPrice, decimal? MaxPrice);
}
