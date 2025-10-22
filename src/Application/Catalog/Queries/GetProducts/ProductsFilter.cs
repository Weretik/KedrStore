namespace Application.Catalog.Queries.GetProducts;

public sealed record ProductsFilter(
    string? SearchTerm,
    ProductCategoryId? CategoryId,
    decimal? MinPrice,
    decimal? MaxPrice,
    string? Manufacturer);
