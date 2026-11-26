using BuildingBlocks.Application.Caching;
using Catalog.Application.Features.Categories.Shared.Tree;
using Catalog.Application.Features.SiteModels.Shared.State;
using Catalog.Contracts.Category;

namespace Catalog.Application.Features.CategoryPlacement.Commands.MoveCategory;

/// <summary>Puts any category under another one or at the top (spec catalog/012 MC-03, MC-09).</summary>
public sealed record MoveCategoryCommand(int CategoryId, int? ParentId)
    : ICommand<Result<AdminCategoryResponse>>, IInvalidatesReadCache
{
    public IReadOnlyCollection<string> InvalidatedCacheTags => [CategoryTreeCache.Tag, SiteModelStateReader.Tag];
}
