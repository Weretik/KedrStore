namespace Sales.Application.Contracts.Pricing;

public interface IPricePolicyProvider
{
    Task<PricePolicy> GetPolicyAsync(
        Guid? identityUserId,
        CancellationToken cancellationToken);
}
