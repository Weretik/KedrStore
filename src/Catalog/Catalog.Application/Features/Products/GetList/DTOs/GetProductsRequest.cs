namespace Catalog.Application.Features.Products.GetList;


public sealed record GetProductsRequest
{
    public string? SearchTerm { get; init; }

    public int? CategoryId { get; init; }
    public bool? InStock { get; init; } = true;
    public bool? IsSale { get; init; }
    public bool? IsNew { get; init; }

    public int PriceTypeId { get; init; } = 10;
    public decimal? PriceFrom { get; init; }
    public decimal? PriceTo { get; init; }

    public ProductSort Sort { get; init; } = ProductSort.NameAsc;

    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
