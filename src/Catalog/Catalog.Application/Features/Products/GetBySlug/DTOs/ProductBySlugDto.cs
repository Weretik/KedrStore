namespace Catalog.Application.Features.Products.GetBySlug.DTOs;

public record  ProductBySlugDto
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    /// <summary>Current slug; differs from the requested one when a former slug was used.</summary>
    public string ProductSlug { get; init; } = null!;
    /// <summary>First photo URL; an empty string when the product has no photo.</summary>
    public string Photo { get; init; } = null!;
    /// <summary>Scheme URL; an empty string when the product has no scheme.</summary>
    public string Scheme { get; init; } = null!;
    /// <summary>All photo URLs in gallery order; the first equals <see cref="Photo"/>.</summary>
    public IReadOnlyList<string> Photos { get; init; } = [];
    public decimal Stock { get; init; }

    public string CategoryName { get; init; } = null!;
    public string CategorySlug { get; init; } = null!;

    public int QuantityInPack { get; init; }

    public decimal? Price { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }

    public IReadOnlyList<CategoryBreadcrumbDto> Breadcrumbs { get; init; } = [];

    /// <summary>The variant group with at least two variants on the site; null otherwise.</summary>
    public ProductVariantsDto? Variants { get; init; }

    /// <summary>Confirmed attribute values for «Характеристики», in attribute order.</summary>
    public IReadOnlyList<ProductCharacteristicDto> Characteristics { get; init; } = [];
}
