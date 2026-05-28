namespace Sales.Application.Contracts.Pricing;

public interface IPricePolicyProvider
{
    Task<PricePolicy> GetPolicyAsync(
        string? counterpartyId,
        CancellationToken cancellationToken);
}
