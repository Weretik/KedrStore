namespace Sales.Infrastructure.Pricing;

internal sealed class DefaultPricePolicyProvider(
    IReadSalesDbContext salesDbContext,
    IOptionsSnapshot<CatalogPricingOptions> pricingOptions) : IPricePolicyProvider
{
    public Task<PricePolicy> GetPolicyAsync(
        string? counterpartyId,
        CancellationToken cancellationToken)
    {
        return string.IsNullOrWhiteSpace(counterpartyId)
            ? Task.FromResult(CreateRetailPolicy())
            : GetCounterpartyPolicyAsync(counterpartyId, cancellationToken);
    }

    private async Task<PricePolicy> GetCounterpartyPolicyAsync(
        string counterpartyId,
        CancellationToken cancellationToken)
    {
        var trimmedCounterpartyId = counterpartyId.Trim();

        var counterparty = await salesDbContext.Counterparties
            .AsNoTracking()
            .Where(x => x.Id == trimmedCounterpartyId)
            .Select(x => new
            {
                x.DefaultPriceTypeId
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (counterparty is null)
        {
            return CreateRetailPolicy();
        }

        var categoryPriceTypes = await salesDbContext.CounterpartyCategoryPriceTypes
            .AsNoTracking()
            .Where(rule => rule.CounterpartyId == trimmedCounterpartyId)
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
