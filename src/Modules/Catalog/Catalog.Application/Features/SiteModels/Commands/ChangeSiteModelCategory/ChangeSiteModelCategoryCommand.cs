using BuildingBlocks.Application.Caching;
using Catalog.Application.Features.SiteModels.Shared.State;
using Catalog.Contracts.SiteModels;

namespace Catalog.Application.Features.SiteModels.Commands.ChangeSiteModelCategory;

/// <summary>Moves a model, with all its products on the site, to another final category (spec catalog/007 R-22).</summary>
public sealed record ChangeSiteModelCategoryCommand(int ModelId, ChangeSiteModelCategoryRequest Request)
    : ICommand<Result<SiteModelDto>>, IInvalidatesReadCache
{
    public IReadOnlyCollection<string> InvalidatedCacheTags => [SiteModelStateReader.Tag];
}
