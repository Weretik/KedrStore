namespace Catalog.Api.Contracts.Products;

public sealed record GetProductsRequest(
// Filtering
    string? SearchTerm,
    int? CategoryId,
    decimal? Stock,
    int? ProductTypeId,
// Sorting
    ProductSortKey Key = ProductSortKey.Name,
    bool Desc = false,
// Pricing
    string PriceType = "price_10",
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
// Pagination
    int CurrentPage = 1,
    int PageSize = 12,
    bool All = false);
