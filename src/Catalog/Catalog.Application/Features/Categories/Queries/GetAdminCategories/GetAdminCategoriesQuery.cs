using Catalog.Application.Features.Categories.Shared;
using Catalog.Contracts.Category;

namespace Catalog.Application.Features.Categories.Queries.GetAdminCategories;

/// <param name="Deleted"><c>true</c> lists soft-deleted categories only (Admin role only).</param>
public sealed record GetAdminCategoriesQuery(bool Deleted = false) : IQuery<Result<IReadOnlyList<AdminCategoryResponse>>>;
