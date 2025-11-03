using Domain.Catalog.ValueObjects;

namespace Application.Catalog.GetProducts;

public sealed record ProductFilter(
    string? SearchTerm = null,
    ProductCategoryId? CategoryId = null,
    decimal? Stock = null);
