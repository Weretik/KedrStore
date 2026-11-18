using BuildingBlocks.Application.Caching;
using Catalog.Application.Features.SiteModels.Shared.State;
using Catalog.Contracts.SiteModels;

namespace Catalog.Application.Features.SiteModels.Commands.SetSiteModelAxes;

public sealed record SetSiteModelAxesCommand(int ModelId, SetSiteModelAxesRequest Request)
    : ICommand<Result<SiteModelDto>>, IInvalidatesReadCache
{
    public IReadOnlyCollection<string> InvalidatedCacheTags => [SiteModelStateReader.Tag];

    public const int MaxAxes = 6;
}
