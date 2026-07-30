using Catalog.Contracts.Category;

namespace Catalog.Application.Features.Category.GetBySlug;

public sealed record GetCategoryBySlugQuery(string Slug, string Lang)
    : IQuery<Result<CategoryDetailsResponse>>;
