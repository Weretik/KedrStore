using Catalog.Contracts.Category;

namespace Catalog.Application.Features.Categories.Queries.GetCategoryBySlug;

public sealed record GetCategoryBySlugQuery(string Slug, string Lang)
    : IQuery<Result<CatalogLookup<CategoryDetailsResponse>>>;
