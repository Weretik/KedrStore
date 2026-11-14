namespace Sales.Application.Abstractions.Pricing;

public sealed record PricePolicy(
    int DefaultPriceTypeId,
    IReadOnlyCollection<CategoryPriceType> CategoryPriceTypes);
