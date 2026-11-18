using BuildingBlocks.Application.Caching;
using Catalog.Application.Features.SiteModels.Shared.State;
using Catalog.Contracts.Products.Media;

namespace Catalog.Application.Features.ProductMedia.Commands.SetProductMedia;

public sealed record SetProductMediaCommand(int ProductId, SetProductMediaRequest Request)
    : ICommand<Result<ProductMediaDto>>, IInvalidatesReadCache
{
    public IReadOnlyCollection<string> InvalidatedCacheTags => [SiteModelStateReader.Tag];
}
