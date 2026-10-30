namespace Catalog.Application.Features.Products.GetBySlug.DTOs;

public record  ProductBySlugDto
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    /// <summary>Current slug; differs from the requested one when a former slug was used.</summary>
    public string ProductSlug { get; init; } = null!;
    public string Photo { get; init; } = null!;
    public string Scheme { get; init; } = null!;
    public decimal Stock { get; init; }

    public string CategoryName { get; init; } = null!;
    public string CategorySlug { get; init; } = null!;

    public int QuantityInPack { get; init; }

    public decimal? Price { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }

    public IReadOnlyList<CategoryBreadcrumbDto> Breadcrumbs { get; init; } = [];
}
