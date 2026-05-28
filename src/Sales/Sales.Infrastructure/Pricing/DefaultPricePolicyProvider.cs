namespace Sales.Infrastructure.Pricing;

internal sealed class DefaultPricePolicyProvider(
    IReadSalesDbContext salesDbContext,
    IOptionsSnapshot<CatalogPricingOptions> pricingOptions) : IPricePolicyProvider
{
    public ValueTask<PricePolicy> GetPolicyAsync(
        CatalogRequest request,
        CancellationToken cancellationToken)
    {
        return string.IsNullOrWhiteSpace(request.CounterpartyId)
            ? ValueTask.FromResult(CreateRetailPolicy())
            : GetCounterpartyPolicyAsync(request.CounterpartyId, cancellationToken);
    }

    private async ValueTask<PricePolicy> GetCounterpartyPolicyAsync(
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
