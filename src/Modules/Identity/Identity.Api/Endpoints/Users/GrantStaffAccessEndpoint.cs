using Identity.Application.Features.Users.Commands.GrantStaffAccess;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Identity.Api.Endpoints.Users;

internal static class GrantStaffAccessEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPost("{userId:guid}/staff-access", Handle)
            .WithName("grantStaffAccess")
            .Produces<StaffAccountResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

    /// <summary>Gives an existing account a staff role and a new password; it keeps its customer role.</summary>
    private static async Task<IResult> Handle(
        Guid userId, [FromBody] GrantStaffAccessRequest request, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GrantStaffAccessCommand(userId, request.FullName, request.Role, request.Password), cancellationToken);
        return result.Status == Ardalis.Result.ResultStatus.Ok
            ? TypedResults.Ok(StaffAccountResponse.From(result.Value))
            : result.ToProblem(http);
    }
}
