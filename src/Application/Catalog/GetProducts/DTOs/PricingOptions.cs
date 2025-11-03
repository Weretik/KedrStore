namespace Application.Catalog.GetProducts;

public sealed record PricingOptions(
    int PriceTypeId = 10,
    decimal? MinPrice = null,
    decimal? MaxPrice = null);
