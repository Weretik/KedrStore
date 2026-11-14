using Catalog.Contracts.SiteModels;

namespace Catalog.Application.Features.SiteModels.Commands.AddSiteModelProducts;

public sealed record AddSiteModelProductsCommand(int ModelId, AddSiteModelProductsRequest Request)
    : ICommand<Result<SiteModelDto>>
{
    public const int MaxProducts = 200;
}
