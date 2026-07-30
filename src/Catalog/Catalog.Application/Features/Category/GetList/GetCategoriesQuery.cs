using Catalog.Application.Features.Category.GetList.DTOs;

namespace Catalog.Application.Features.Category.GetList;

public sealed record GetCategoriesQuery(CategoryFilter Filter, string Lang = "uk")
    : IQuery<Result<IReadOnlyList<CategoryTreeDto>>>;
