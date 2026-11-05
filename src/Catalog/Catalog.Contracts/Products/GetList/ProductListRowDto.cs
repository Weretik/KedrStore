namespace Catalog.Contracts.Products.GetList;

public sealed record ProductListRowDto
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

    /// <summary>With <c>groupVariants</c>: on-site variants of the row's group, this one included; null for a single product.</summary>
    public int? VariantCount { get; init; }

    /// <summary>With <c>groupVariants</c>: the colors of the group's on-site variants in dictionary order.</summary>
    public IReadOnlyList<ProductSwatchDto>? Swatches { get; init; }
}

/// <summary>A color of a variant: a #RRGGBB circle, a texture picture, or only the name when neither is set.</summary>
public sealed record ProductSwatchDto(string Slug, string Name, string? Hex, string? TextureUrl);
