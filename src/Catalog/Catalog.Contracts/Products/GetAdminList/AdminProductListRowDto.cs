namespace Catalog.Contracts.Products.GetAdminList;

public sealed record AdminProductListRowDto
{
    public int Id { get; init; }
    public string NameUk { get; init; } = null!;
    public string NameRu { get; init; } = null!;
    public string ProductSlug { get; init; } = null!;
    public string Photo { get; init; } = null!;
    public int? CategoryId { get; init; }
    public bool InStock { get; init; }
    public bool IsSale { get; init; }
    public bool IsNew { get; init; }
    public bool ExportToSite { get; init; }
    public decimal? Price { get; init; }
    public decimal Stock { get; init; }
    public int QuantityInPack { get; init; }
}
