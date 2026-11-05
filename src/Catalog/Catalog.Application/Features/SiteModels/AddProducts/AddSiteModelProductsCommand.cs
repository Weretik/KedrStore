using Catalog.Contracts.SiteModels;

namespace Catalog.Application.Features.SiteModels.AddProducts;

public sealed record AddSiteModelProductsCommand(int ModelId, AddSiteModelProductsRequest Request)
    : ICommand<Result<SiteModelDto>>
{
    public const int MaxProducts = 200;
}
