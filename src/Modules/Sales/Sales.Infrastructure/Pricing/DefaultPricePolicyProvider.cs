namespace Sales.Infrastructure.Pricing;

internal sealed class DefaultPricePolicyProvider(
    IReadSalesDbContext salesDbContext,
    IOptionsSnapshot<CatalogPricingOptions> pricingOptions) : IPricePolicyProvider
{
    public Task<PricePolicy> GetPolicyAsync(
        Guid? identityUserId,
        CancellationToken cancellationToken)
    {
        return identityUserId is null
            ? Task.FromResult(CreateRetailPolicy())
            : GetCounterpartyPolicyAsync(identityUserId.Value, cancellationToken);
    }

    private async Task<PricePolicy> GetCounterpartyPolicyAsync(
        Guid identityUserId,
        CancellationToken cancellationToken)
    {
        var counterparty = await salesDbContext.Counterparties
            .AsNoTracking()
            .Where(x => x.IdentityUserId == identityUserId)
            .Select(x => new
            {
                x.Id,
                x.DefaultPriceTypeId
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (counterparty is null)
        {
            return CreateRetailPolicy();
        }

        var categoryPriceTypes = await salesDbContext.CounterpartyCategoryPriceTypes
            .AsNoTracking()
            .Where(rule => rule.CounterpartyId == counterparty.Id)
            .Select(rule => new CategoryPriceType(rule.CategoryId, rule.PriceTypeId))
            .ToArrayAsync(cancellationToken);

        return new PricePolicy(
            DefaultPriceTypeId: counterparty.DefaultPriceTypeId,
            CategoryPriceTypes: categoryPriceTypes);
    }

    private PricePolicy CreateRetailPolicy()
    {
        return new PricePolicy(
            DefaultPriceTypeId: pricingOptions.Value.RetailPriceTypeId,
            CategoryPriceTypes: []);
    }
}
