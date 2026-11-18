using BuildingBlocks.Application.Caching;
using Catalog.Application.Features.SiteModels.Shared.State;
using Catalog.Contracts.SiteModels;

namespace Catalog.Application.Features.SiteModels.Commands.RemoveSiteModelProduct;

public sealed record RemoveSiteModelProductCommand(int ModelId, int ProductId) : ICommand<Result<SiteModelDto>>, IInvalidatesReadCache
{
    public IReadOnlyCollection<string> InvalidatedCacheTags => [SiteModelStateReader.Tag];
}
