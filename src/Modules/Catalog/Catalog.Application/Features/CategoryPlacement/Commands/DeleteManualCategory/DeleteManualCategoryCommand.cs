using BuildingBlocks.Application.Caching;
using Catalog.Application.Features.Categories.Shared.Tree;
using Catalog.Application.Features.SiteModels.Shared.State;
using Catalog.Contracts.Category;

namespace Catalog.Application.Features.CategoryPlacement.Commands.DeleteManualCategory;

/// <summary>Deletes an empty category made by hand (MC-04).</summary>
public sealed record DeleteManualCategoryCommand(int CategoryId)
    : ICommand<Result<bool>>, IInvalidatesReadCache
{
    public IReadOnlyCollection<string> InvalidatedCacheTags => [CategoryTreeCache.Tag, SiteModelStateReader.Tag];
}
