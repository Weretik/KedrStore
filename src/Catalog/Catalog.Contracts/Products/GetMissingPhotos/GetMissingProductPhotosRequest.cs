using Catalog.Contracts.Products.GetList;

namespace Catalog.Contracts.Products.GetMissingPhotos;

public sealed record GetMissingProductPhotosRequest
{
    public string? SearchTerm { get; init; }
    public int? CategoryId { get; init; }
    public bool? InStock { get; init; }
    public bool? IsSale { get; init; }
    public bool? IsNew { get; init; }
    public bool? ExportToSite { get; init; }
    public ProductSort Sort { get; init; } = ProductSort.IdAsc;
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
