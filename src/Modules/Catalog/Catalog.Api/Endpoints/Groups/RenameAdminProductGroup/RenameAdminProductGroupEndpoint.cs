using Catalog.Application.Features.Groups.Commands.RenameProductGroup;
using Catalog.Contracts.Groups;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Catalog.Api.Endpoints.Groups.RenameAdminProductGroup;

internal static class RenameAdminProductGroupEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPut("{id:int}", Handle)
            .WithName("renameAdminProductGroup")
            .Produces<AdminProductGroupDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromRoute] int id, [FromBody] RenameProductGroupRequest request, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
        => (await sender.Send(new RenameProductGroupCommand(id, request), cancellationToken)).ToHttpResult(http);
}
