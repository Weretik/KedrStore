using Catalog.Contracts.Category;

namespace Catalog.Application.Features.Category.Content.Get;

public sealed record GetCategoryContentQuery(int CategoryId) : IQuery<Result<AdminCategoryContentResponse>>;
