namespace Application.Catalog.Queries.GetCategories;

public sealed record GetCategoriesQuery  : IQuery<Result<List<CategoryDto>>> { }
