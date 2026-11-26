using BuildingBlocks.Application.Caching;
using Catalog.Application.Features.Categories.Shared.Tree;
using Catalog.Application.Features.SiteModels.Shared.State;
using Catalog.Contracts.Category;

namespace Catalog.Application.Features.CategoryPlacement.Commands.AddCategorySiteModels;

/// <summary>Puts site models into a final category (MC-07).</summary>
public sealed record AddCategorySiteModelsCommand(int CategoryId, IReadOnlyList<int> ModelIds)
    : ICommand<Result<bool>>, IInvalidatesReadCache
{
    public IReadOnlyCollection<string> InvalidatedCacheTags => [CategoryTreeCache.Tag, SiteModelStateReader.Tag];
}
