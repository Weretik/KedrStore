namespace Application.Catalog.Queries.GetProducts;

public sealed record GetProductsQuery(ProductsFilter Filter, PageRequest PageRequest, string? Sort) : IQuery<Result<PaginatedList<ProductDto>>>;
