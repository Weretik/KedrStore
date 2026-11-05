using Catalog.Contracts.Category;

namespace Catalog.Application.Features.Category.SiteVisibility;

public sealed record SetCategorySiteVisibilityCommand(int CategoryId, bool IsShownOnSite)
    : ICommand<Result<CategorySiteVisibilityResponse>>;
