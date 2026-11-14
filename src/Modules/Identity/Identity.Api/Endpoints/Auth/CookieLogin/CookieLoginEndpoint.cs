using Identity.Application.Features.Auth.Commands.CookieLogin;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Identity.Api.Endpoints.Auth.Shared;

namespace Identity.Api.Endpoints.Auth.CookieLogin;

internal static class CookieLoginEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPost("login", Handle)
            .WithName("loginCookieSession")
            .AllowAnonymous()
            .RequireRateLimiting("AuthLogin")
            .Produces<SessionMeResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

    private static async Task<IResult> Handle(
        [FromBody] SessionLoginRequest request, ISender sender, SessionCookieWriter cookies, HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CookieLoginCommand(request.Email, request.Password), cancellationToken);
        if (result.Status != Ardalis.Result.ResultStatus.Ok || result.Value is null)
            return result.ToProblem(http);

        cookies.AppendCsrfToken(http.Response);
        return TypedResults.Ok(SessionMeResponse.From(result.Value));
    }
}
