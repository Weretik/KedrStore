namespace Sales.Api.Contracts.Catalog;

public sealed record CatalogProductsRequest
{
    public int? CategoryId { get; init; }
    public bool? InStock { get; init; } = true;
    public bool? IsSale { get; init; }
    public bool? IsNew { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
