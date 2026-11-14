using Identity.Application.Features.Auth.Commands.SessionLogout;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
namespace Identity.Api.Endpoints.Auth.MobileLogout;

internal static class MobileLogoutEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPost("logout", Handle)
            .WithName("logoutMobile")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

    private static async Task<IResult> Handle(
        [FromBody] MobileLogoutRequest request, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SessionLogoutCommand(request.RefreshToken), cancellationToken);
        return result.Status == Ardalis.Result.ResultStatus.Ok ? TypedResults.NoContent() : result.ToProblem(http);
    }
}
