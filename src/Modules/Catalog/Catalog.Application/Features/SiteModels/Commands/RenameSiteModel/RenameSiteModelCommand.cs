using BuildingBlocks.Application.Caching;
using Catalog.Application.Features.SiteModels.Shared.State;
using Catalog.Contracts.SiteModels;

namespace Catalog.Application.Features.SiteModels.Commands.RenameSiteModel;

public sealed record RenameSiteModelCommand(int ModelId, RenameSiteModelRequest Request) : ICommand<Result<SiteModelDto>>, IInvalidatesReadCache
{
    public IReadOnlyCollection<string> InvalidatedCacheTags => [SiteModelStateReader.Tag];
}
