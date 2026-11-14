using Identity.Application.Features.Auth.Commands.CookieLogout;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Identity.Api.Endpoints.Auth.Shared;

namespace Identity.Api.Endpoints.Auth.CookieLogout;

internal static class CookieLogoutEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPost("logout", Handle)
            .WithName("logoutCookieSession")
            .RequireAuthorization()
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized);

    private static async Task<IResult> Handle(
        ISender sender, SessionCookieWriter cookies, HttpContext http, CancellationToken cancellationToken)
    {
        await sender.Send(new CookieLogoutCommand(), cancellationToken);
        cookies.DeleteCsrfToken(http.Response);
        return TypedResults.NoContent();
    }
}
