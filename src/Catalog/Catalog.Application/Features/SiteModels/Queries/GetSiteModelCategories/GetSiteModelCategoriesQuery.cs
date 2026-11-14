using Catalog.Contracts.SiteModels;

namespace Catalog.Application.Features.SiteModels.Queries.GetSiteModelCategories;

public sealed record GetSiteModelCategoriesQuery : IQuery<Result<List<SiteModelCategoryDto>>>;
