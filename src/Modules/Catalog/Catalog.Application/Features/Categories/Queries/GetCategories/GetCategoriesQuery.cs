using Catalog.Application.Features.Categories.Shared;
namespace Catalog.Application.Features.Categories.Queries.GetCategories;

public sealed record GetCategoriesQuery(CategoryFilter Filter, string Lang = "uk")
    : IQuery<Result<IReadOnlyList<CategoryTreeDto>>>;
