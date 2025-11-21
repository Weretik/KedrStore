using Application.Catalog.Shared;

namespace Application.Catalog.GetCategories;

public sealed record GetCategoriesQuery(CategoryFilter Filter)  : IQuery<Result<IReadOnlyList<CategoryTreeDto>>> { }
