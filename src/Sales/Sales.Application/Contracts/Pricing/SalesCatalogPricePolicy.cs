namespace Sales.Application.Contracts.Pricing;

public sealed record SalesCatalogPricePolicy(
    int DefaultPriceTypeId,
    IReadOnlyCollection<SalesCatalogCategoryPriceType> CategoryPriceTypes);
