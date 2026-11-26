using BuildingBlocks.Application.Caching;
using Catalog.Application.Features.Categories.Shared.Tree;
using Catalog.Application.Features.SiteModels.Shared.State;
using Catalog.Contracts.Category;

namespace Catalog.Application.Features.CategoryPlacement.Commands.CreateManualCategory;

/// <summary>A category made by hand (spec catalog/012 MC-01, MC-02); it changes the tree and the model categories.</summary>
public sealed record CreateManualCategoryCommand(CreateCategoryRequest Request)
    : ICommand<Result<AdminCategoryResponse>>, IInvalidatesReadCache
{
    public IReadOnlyCollection<string> InvalidatedCacheTags => [CategoryTreeCache.Tag, SiteModelStateReader.Tag];
}
