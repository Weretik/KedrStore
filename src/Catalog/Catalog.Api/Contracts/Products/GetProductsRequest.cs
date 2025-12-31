using Catalog.Application.Shared;

namespace Catalog.Api.Contracts.Products;

public sealed record GetProductsRequest
{
    // Pagination
    public int CurrentPage { get; init; } = 1;
    public int PageSize { get; init; } = 12;
    public bool All { get; init; } = false;

    // Filtering
    public string? SearchTerm { get; init; }
    public int? CategoryId { get; init; }
    public decimal? Stock { get; init; }
    public int? ProductTypeId { get; init; }

    // Sorting
    // examples: "name", "-name", "price", "-price"
    public ProductSortKey Key { get; init; } = ProductSortKey.Name;
    public bool Desc { get; init; } = false;

    // Pricing
    public string PriceType { get; init; } = "price_10";
    public decimal? MinPrice { get; init; } = null;
    public decimal? MaxPrice { get; init; } = null;
}
