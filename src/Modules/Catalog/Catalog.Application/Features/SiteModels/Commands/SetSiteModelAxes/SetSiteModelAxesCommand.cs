using Catalog.Contracts.SiteModels;

namespace Catalog.Application.Features.SiteModels.Commands.SetSiteModelAxes;

public sealed record SetSiteModelAxesCommand(int ModelId, SetSiteModelAxesRequest Request)
    : ICommand<Result<SiteModelDto>>
{
    public const int MaxAxes = 6;
}
