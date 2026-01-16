namespace Catalog.Application.Features.Products.GetById.DTOs;


public record  ProductBySlugDto
{
    public int Id { get; init; }
    public string Name { get; init; } = null!;
    public string Photo { get; init; } = null!;
    public string Scheme { get; init; } = null!;
    public decimal Stock { get; init; }

    public string CategoryName { get; init; } = null!;
    public string CategorySlug { get; init; } = null!;

    public decimal Price { get; init; }
}
