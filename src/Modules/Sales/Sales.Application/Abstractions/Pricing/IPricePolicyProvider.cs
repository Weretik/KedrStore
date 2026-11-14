namespace Sales.Application.Abstractions.Pricing;

public interface IPricePolicyProvider
{
    Task<PricePolicy> GetPolicyAsync(
        Guid? identityUserId,
        CancellationToken cancellationToken);
}
