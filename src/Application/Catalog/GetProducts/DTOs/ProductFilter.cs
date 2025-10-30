using Domain.Catalog.ValueObjects;

namespace Application.Catalog.GetProducts;

public sealed record ProductFilter(
    string? SearchTerm,
    ProductCategoryId? CategoryId,
    decimal? MinPrice,
    decimal? MaxPrice,
    decimal? Stock);
