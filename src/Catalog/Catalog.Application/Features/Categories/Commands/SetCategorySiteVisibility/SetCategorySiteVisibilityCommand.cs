using Catalog.Application.Features.Categories.Shared;
using Catalog.Contracts.Category;

namespace Catalog.Application.Features.Categories.Commands.SetCategorySiteVisibility;

public sealed record SetCategorySiteVisibilityCommand(int CategoryId, bool IsShownOnSite)
    : ICommand<Result<CategorySiteVisibilityResponse>>;
