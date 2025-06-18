namespace Application.Catalog.Queries;

public record GetProductsByCategoryQuery(CategoryId CategoryId)
    : IQuery<AppResult<List<ProductDto>>>;


