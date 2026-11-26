using BuildingBlocks.Application.Caching;
using Catalog.Application.Features.Categories.Shared.Tree;
using Catalog.Application.Features.SiteModels.Shared.State;
using Catalog.Contracts.Category;

namespace Catalog.Application.Features.Categories.Commands.SetCategorySearchSettings;

/// <summary>The public tree and the site models state carry the search mark, so the command drops both caches.</summary>
public sealed record SetCategorySearchSettingsCommand(int CategoryId, bool IsClosedFromSearch, long? ShareImageMediaFileId)
    : ICommand<Result<AdminCategoryResponse>>, IInvalidatesReadCache
{
    public IReadOnlyCollection<string> InvalidatedCacheTags => [CategoryTreeCache.Tag, SiteModelStateReader.Tag];
}
