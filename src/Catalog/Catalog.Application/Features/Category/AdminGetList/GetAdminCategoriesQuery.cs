using Catalog.Contracts.Category;

namespace Catalog.Application.Features.Category.AdminGetList;

/// <param name="Deleted"><c>true</c> lists soft-deleted categories only (Admin role only).</param>
public sealed record GetAdminCategoriesQuery(bool Deleted = false) : IQuery<Result<IReadOnlyList<AdminCategoryResponse>>>;
