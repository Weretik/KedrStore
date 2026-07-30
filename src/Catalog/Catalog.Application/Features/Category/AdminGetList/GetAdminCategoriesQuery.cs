using Catalog.Contracts.Category;

namespace Catalog.Application.Features.Category.AdminGetList;

public sealed record GetAdminCategoriesQuery : IQuery<Result<IReadOnlyList<AdminCategoryResponse>>>;
