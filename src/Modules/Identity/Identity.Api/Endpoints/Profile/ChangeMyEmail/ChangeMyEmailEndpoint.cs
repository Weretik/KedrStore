using Identity.Contracts.Authorization;
using Identity.Application.Features.Profile.Commands.ChangeOwnEmail;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Identity.Api.Endpoints.Profile.ChangeMyEmail;

internal static class ChangeMyEmailEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPut("email", Handle)
            .WithName("changeMyEmail")
            .RequireAuthorization(PolicyNames.RequireAdminRole)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

    private static async Task<IResult> Handle(
        [FromBody] ChangeOwnEmailRequest request, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ChangeOwnEmailCommand(request.Email, request.CurrentPassword), cancellationToken);
        return result.Status == Ardalis.Result.ResultStatus.Ok ? TypedResults.NoContent() : result.ToProblem(http);
    }
}
