using Catalog.Application.Features.Categories.Shared;
using Catalog.Contracts.Category;

namespace Catalog.Application.Features.Categories.Queries.GetCategoryContent;

public sealed record GetCategoryContentQuery(int CategoryId) : IQuery<Result<AdminCategoryContentResponse>>;
