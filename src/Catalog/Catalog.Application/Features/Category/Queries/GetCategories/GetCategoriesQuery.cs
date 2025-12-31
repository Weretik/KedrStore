namespace Catalog.Application.Features.Category.Queries.GetCategories;

public sealed record GetCategoriesQuery(CategoryFilter Filter)  : IQuery<Result<IReadOnlyList<CategoryTreeDto>>> { }
