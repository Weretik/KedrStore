using BuildingBlocks.Application.Caching;
using Catalog.Application.Features.Categories.Shared.Tree;
using Catalog.Application.Features.SiteModels.Shared.State;
using Catalog.Contracts.Category;

namespace Catalog.Application.Features.Categories.Commands.SetCategorySiteNames;

/// <summary>A site name changes the public tree and the site models state, so it drops both caches.</summary>
public sealed record SetCategorySiteNamesCommand(int CategoryId, string? NameUk, string? NameRu)
    : ICommand<Result<AdminCategoryResponse>>, IInvalidatesReadCache
{
    public IReadOnlyCollection<string> InvalidatedCacheTags => [CategoryTreeCache.Tag, SiteModelStateReader.Tag];
}
