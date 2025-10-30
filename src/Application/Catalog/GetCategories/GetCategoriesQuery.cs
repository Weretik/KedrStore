namespace Application.Catalog.GetCategories;

public sealed record GetCategoriesQuery  : IQuery<Result<IReadOnlyList<CategoryTreeDto>>> { }
