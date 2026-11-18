using BuildingBlocks.Application.Caching;
using Catalog.Application.Features.SiteModels.Shared.State;
using Catalog.Contracts.Groups;

namespace Catalog.Application.Features.Groups.Commands.CreateProductGroup;

public sealed record CreateProductGroupCommand(CreateProductGroupRequest Request) : ICommand<Result<AdminProductGroupDto>>, IInvalidatesReadCache
{
    public IReadOnlyCollection<string> InvalidatedCacheTags => [SiteModelStateReader.Tag];

    public const int MaxProducts = 200;
}
