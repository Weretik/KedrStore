using Identity.Application.Features.Users.Commands.GrantStaffAccess;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Identity.Api.Endpoints.Users.Shared;

namespace Identity.Api.Endpoints.Users.GrantStaffAccess;

internal static class GrantStaffAccessEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPost("{userId:guid}/staff-access", Handle)
            .WithName("grantStaffAccess")
            .WithSummary("Give an existing account a staff role and a password")
            .WithDescription("The account gets the role, the name, and a new password; its email stays. A customer keeps the customer role, orders and prices, and signs in on the site with the new password too. Its active refresh sessions end. Later 1C customer syncs no longer change the email, name, password, or block state of a staff account, and a customer removed from 1C keeps the staff account without the customer role.")
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
