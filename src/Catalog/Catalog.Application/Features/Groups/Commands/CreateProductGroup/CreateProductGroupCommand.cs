using Catalog.Contracts.Groups;

namespace Catalog.Application.Features.Groups.Commands.CreateProductGroup;

public sealed record CreateProductGroupCommand(CreateProductGroupRequest Request) : ICommand<Result<AdminProductGroupDto>>
{
    public const int MaxProducts = 200;
}
