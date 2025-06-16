namespace Application.UseCases.Catalog.Queries;

public record GetProductsByCategoryQuery(CategoryId CategoryId)
    : IQuery<AppResult<List<ProductDto>>>;


