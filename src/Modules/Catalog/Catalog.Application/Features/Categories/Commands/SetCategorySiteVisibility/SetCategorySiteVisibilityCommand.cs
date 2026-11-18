using BuildingBlocks.Application.Caching;
using Catalog.Application.Features.Categories.Shared.Tree;
using Catalog.Application.Features.SiteModels.Shared.State;
using Catalog.Contracts.Category;

namespace Catalog.Application.Features.Categories.Commands.SetCategorySiteVisibility;

/// <summary>Showing or hiding a category changes the public tree, so it drops <see cref="CategoryTreeCache.Tag"/>
/// and the site models state (<see cref="SiteModelStateReader.Tag"/>).</summary>
public sealed record SetCategorySiteVisibilityCommand(int CategoryId, bool IsShownOnSite)
    : ICommand<Result<CategorySiteVisibilityResponse>>, IInvalidatesReadCache
{
    public IReadOnlyCollection<string> InvalidatedCacheTags => [CategoryTreeCache.Tag, SiteModelStateReader.Tag];
}
