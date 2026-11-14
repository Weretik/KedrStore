using Catalog.Application.Features.Groups.Commands.SetProductGroupMain;
using Catalog.Contracts.Groups;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Catalog.Api.Endpoints.Groups.SetAdminProductGroupMain;

internal static class SetAdminProductGroupMainEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPut("{id:int}/main", Handle)
            .WithName("setAdminProductGroupMain")
            .Produces<AdminProductGroupDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

    private static async Task<IResult> Handle(
        [FromRoute] int id, [FromBody] SetProductGroupMainRequest request, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
        => (await sender.Send(new SetProductGroupMainCommand(id, request), cancellationToken)).ToHttpResult(http);
}
