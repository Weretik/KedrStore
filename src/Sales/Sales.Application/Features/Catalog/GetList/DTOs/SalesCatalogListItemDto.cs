namespace Sales.Application.Features.Catalog.GetList.DTOs;

public sealed record SalesCatalogListItemDto
{
    public int ProductId { get; init; }
    public int CategoryId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string ProductSlug { get; init; } = string.Empty;
    public string? Photo { get; init; }
    public bool InStock { get; init; }
    public bool IsSale { get; init; }
    public bool IsNew { get; init; }
    public decimal? Price { get; init; }
}
