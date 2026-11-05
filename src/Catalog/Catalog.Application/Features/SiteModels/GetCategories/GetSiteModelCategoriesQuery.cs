using Catalog.Contracts.SiteModels;

namespace Catalog.Application.Features.SiteModels.GetCategories;

public sealed record GetSiteModelCategoriesQuery : IQuery<Result<List<SiteModelCategoryDto>>>;
