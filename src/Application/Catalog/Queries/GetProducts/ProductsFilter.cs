namespace Application.Catalog.Queries.GetProducts;

public sealed record ProductsFilter(
    string? SearchTerm,
    CategoryId? CategoryId,
    decimal? MinPrice,
    decimal? MaxPrice,
    string? Manufacturer);
