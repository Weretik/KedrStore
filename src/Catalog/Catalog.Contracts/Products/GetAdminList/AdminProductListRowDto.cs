using Catalog.Contracts.Attributes;

namespace Catalog.Contracts.Products.GetAdminList;

public sealed record AdminProductListRowDto
{
    public int Id { get; init; }
    public string NameUk { get; init; } = null!;
    public string NameRu { get; init; } = null!;
    public string ProductSlug { get; init; } = null!;
    public string Photo { get; init; } = null!;
    public int? CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public bool InStock { get; init; }
    public bool IsSale { get; init; }
    public bool IsNew { get; init; }
    public bool ExportToSite { get; init; }
    public decimal? Price { get; init; }
    public decimal Stock { get; init; }
    public int QuantityInPack { get; init; }
    public bool IsDeleted { get; init; }
    public DateTimeOffset? DeletedAt { get; init; }
    /// <summary>The variant group of the product, if any (spec catalog/006).</summary>
    public int? GroupId { get; init; }
    public string? GroupName { get; init; }
    /// <summary>Why the product needs review; empty when it does not (accepted products have none).</summary>
    public IReadOnlyList<AdminAttributeProblemDto> Problems { get; init; } = [];
}
