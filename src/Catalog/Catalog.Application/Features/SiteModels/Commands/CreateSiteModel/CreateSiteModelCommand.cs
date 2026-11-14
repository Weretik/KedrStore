using Catalog.Contracts.SiteModels;

namespace Catalog.Application.Features.SiteModels.Commands.CreateSiteModel;

public sealed record CreateSiteModelCommand(CreateSiteModelRequest Request) : ICommand<Result<SiteModelDto>>
{
    public const int MaxProducts = 200;
}
