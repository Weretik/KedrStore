using BuildingBlocks.Application.Caching;
using Catalog.Application.Features.Categories.Shared.Tree;
using Catalog.Application.Features.SiteModels.Shared.State;
using Catalog.Contracts.Category;

namespace Catalog.Application.Features.CategoryPlacement.Commands.ReorderCategory;

/// <summary>Moves a category one place up or down among its neighbours (MC-10).</summary>
public sealed record ReorderCategoryCommand(int CategoryId, string Direction)
    : ICommand<Result<AdminCategoryResponse>>, IInvalidatesReadCache
{
    public IReadOnlyCollection<string> InvalidatedCacheTags => [CategoryTreeCache.Tag, SiteModelStateReader.Tag];
}
