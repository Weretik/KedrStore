namespace Sales.Application.Contracts.Pricing;

public interface IPricePolicyProvider
{
    ValueTask<PricePolicy> GetPolicyAsync(
        CatalogRequest request,
        CancellationToken cancellationToken);
}
