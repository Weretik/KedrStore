using Catalog.Application.Features.Categories.Shared;
using Catalog.Contracts.Category;

namespace Catalog.Application.Features.Categories.Queries.GetAdminCategoryById;

public sealed record GetAdminCategoryByIdQuery(int Id) : IQuery<Result<AdminCategoryResponse>>;
