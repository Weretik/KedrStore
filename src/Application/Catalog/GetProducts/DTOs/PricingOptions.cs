namespace Application.Catalog.GetProducts;

public sealed record PricingOptions(
    string PriceType = "price_10",
    decimal? MinPrice = null,
    decimal? MaxPrice = null);
