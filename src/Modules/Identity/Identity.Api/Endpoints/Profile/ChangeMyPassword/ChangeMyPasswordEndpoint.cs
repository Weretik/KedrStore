using Identity.Contracts.Authorization;
using Identity.Application.Features.Profile.Commands.ChangeOwnPassword;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Identity.Api.Endpoints.Profile.ChangeMyPassword;

internal static class ChangeMyPasswordEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPut("password", Handle)
            .WithName("changeMyPassword")
            .RequireAuthorization(PolicyNames.RequireAdminRole)
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);

    private static async Task<IResult> Handle(
        [FromBody] ChangeOwnPasswordRequest request, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ChangeOwnPasswordCommand(request.CurrentPassword, request.NewPassword), cancellationToken);
        return result.Status == Ardalis.Result.ResultStatus.Ok ? TypedResults.NoContent() : result.ToProblem(http);
    }
}
