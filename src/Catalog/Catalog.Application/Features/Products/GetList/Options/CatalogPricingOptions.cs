namespace Catalog.Application.Features.Products.GetList.Options;

public sealed class CatalogPricingOptions
{
    public const string SectionName = "Catalog:Pricing";

    public int RetailPriceTypeId { get; init; }
}
