using Catalog.Contracts.Groups;

namespace Catalog.Application.Features.Groups.Create;

public sealed record CreateProductGroupCommand(CreateProductGroupRequest Request) : ICommand<Result<AdminProductGroupDto>>
{
    public const int MaxProducts = 200;
}
