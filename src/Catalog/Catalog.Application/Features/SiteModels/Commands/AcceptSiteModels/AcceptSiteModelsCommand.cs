using Catalog.Contracts.SiteModels;

namespace Catalog.Application.Features.SiteModels.Commands.AcceptSiteModels;

/// <summary>Accepts the listed models; returns the health counts after it.</summary>
public sealed record AcceptSiteModelsCommand(AcceptSiteModelsRequest Request) : ICommand<Result<SiteModelHealthDto>>
{
    public const int MaxModels = 500;
}
