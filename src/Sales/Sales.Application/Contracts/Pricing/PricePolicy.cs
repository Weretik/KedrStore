namespace Sales.Application.Contracts.Pricing;

public sealed record PricePolicy(
    int DefaultPriceTypeId,
    IReadOnlyCollection<CategoryPriceType> CategoryPriceTypes);
