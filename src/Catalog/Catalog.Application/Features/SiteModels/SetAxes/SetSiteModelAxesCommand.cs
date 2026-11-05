using Catalog.Contracts.SiteModels;

namespace Catalog.Application.Features.SiteModels.SetAxes;

public sealed record SetSiteModelAxesCommand(int ModelId, SetSiteModelAxesRequest Request)
    : ICommand<Result<SiteModelDto>>
{
    public const int MaxAxes = 6;
}
