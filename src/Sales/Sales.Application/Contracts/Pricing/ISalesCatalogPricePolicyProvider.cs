namespace Sales.Application.Contracts.Pricing;

public interface ISalesCatalogPricePolicyProvider
{
    ValueTask<SalesCatalogPricePolicy> GetPolicyAsync(
        GetSalesCatalogRequest request,
        CancellationToken cancellationToken);
}
