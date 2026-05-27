namespace Sales.Infrastructure.Pricing;

internal sealed class DefaultSalesCatalogPricePolicyProvider(
    IOptionsSnapshot<CatalogPricingOptions> pricingOptions) : ISalesCatalogPricePolicyProvider
{
    public ValueTask<SalesCatalogPricePolicy> GetPolicyAsync(
        GetSalesCatalogRequest request,
        CancellationToken cancellationToken)
    {
        var policy = new SalesCatalogPricePolicy(
            DefaultPriceTypeId: pricingOptions.Value.RetailPriceTypeId,
            CategoryPriceTypes: []);

        return ValueTask.FromResult(policy);
    }
}
