namespace Catalog.Contracts.Pricing;

public sealed class CatalogPricingOptions
{
    public const string SectionName = "Catalog:Pricing";

    public int RetailPriceTypeId { get; init; } = 12;
}
