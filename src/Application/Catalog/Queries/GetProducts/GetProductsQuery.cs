namespace Application.Catalog.Queries.GetProducts;

public sealed record GetProductsQuery(ProductsCriteria Criteria) : IQuery<Result<PaginatedList<ProductDto>>>;


