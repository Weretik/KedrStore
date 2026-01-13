namespace Catalog.Application.Features.Category.GetList;

public sealed record GetCategoriesQuery(CategoryFilter Filter)  : IQuery<Result<IReadOnlyList<CategoryTreeDto>>> { }
