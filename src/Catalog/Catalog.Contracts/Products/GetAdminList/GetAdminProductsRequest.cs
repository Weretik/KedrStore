using Catalog.Contracts.Products.GetList;

namespace Catalog.Contracts.Products.GetAdminList;

public sealed record GetAdminProductsRequest
{
    public string? SearchTerm { get; init; }
    public string? CategorySlug { get; init; }
    public int? CategoryId { get; init; }

    public bool? InStock { get; init; }
    public bool? IsSale { get; init; }
    public bool? IsNew { get; init; }

    public decimal? PriceFrom { get; init; }
    public decimal? PriceTo { get; init; }

    public ProductSort Sort { get; init; } = ProductSort.IdAsc;

    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
