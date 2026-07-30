using Catalog.Contracts.Category;

namespace Catalog.Application.Features.Category.AdminGetById;

public sealed record GetAdminCategoryByIdQuery(int Id) : IQuery<Result<AdminCategoryResponse>>;
