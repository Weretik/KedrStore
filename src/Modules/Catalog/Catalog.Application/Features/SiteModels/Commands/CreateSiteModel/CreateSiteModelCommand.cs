using BuildingBlocks.Application.Caching;
using Catalog.Application.Features.SiteModels.Shared.State;
using Catalog.Contracts.SiteModels;

namespace Catalog.Application.Features.SiteModels.Commands.CreateSiteModel;

public sealed record CreateSiteModelCommand(CreateSiteModelRequest Request) : ICommand<Result<SiteModelDto>>, IInvalidatesReadCache
{
    public IReadOnlyCollection<string> InvalidatedCacheTags => [SiteModelStateReader.Tag];

    public const int MaxProducts = 200;
}
