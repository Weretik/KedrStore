namespace Catalog.Application.Features.Products.Queries.GetProducts;

public sealed record ProductFilter(
    string? SearchTerm = null,
    int? CategoryId = null,
    decimal? Stock = null,
    int? ProductTypeId = null);
