using Identity.Application.Features.Users.Commands.ChangeStaffRole;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Identity.Api.Endpoints.Users;

internal static class ChangeStaffRoleEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPut("{userId:guid}/role", Handle)
            .WithName("changeStaffRole")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

    private static async Task<IResult> Handle(
        Guid userId, [FromBody] ChangeStaffRoleRequest request, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ChangeStaffRoleCommand(userId, request.Role), cancellationToken);
        return result.Status == Ardalis.Result.ResultStatus.Ok ? TypedResults.NoContent() : result.ToProblem(http);
    }
}
