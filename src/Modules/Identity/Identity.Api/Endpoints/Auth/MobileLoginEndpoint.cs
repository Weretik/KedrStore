using Identity.Application.Features.Auth.Commands.SessionLogin;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Identity.Api.Endpoints.Auth;

internal static class MobileLoginEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPost("login", Handle)
            .WithName("loginMobile")
            .AllowAnonymous()
            .RequireRateLimiting("AuthLogin")
            .Produces<MobileTokenResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

    private static async Task<IResult> Handle(
        [FromBody] SessionLoginRequest request, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var command = new SessionLoginCommand(new SessionLoginCommandRequest(request.Email, request.Password));
        var result = await sender.Send(command, cancellationToken);
        if (result.Status != Ardalis.Result.ResultStatus.Ok || result.Value is null)
            return result.ToProblem(http);

        var value = result.Value;
        return TypedResults.Ok(new MobileTokenResponse(value.TokenType, value.AccessToken, value.ExpiresIn, value.RefreshToken));
    }
}
