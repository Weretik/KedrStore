namespace Catalog.Application.Features.Products.GetList;

public record  ProductListRowDto
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public string ProductSlug { get; init; } = null!;
    public string Photo { get; init; } = null!;
    public int? CategoryId { get; init; }
    public bool InStock { get; init; }
    public bool IsSale { get; init; }
    public bool IsNew { get; init; }
    public decimal? Price { get; init; }
}
