using Identity.Application.Features.Auth.Commands.SessionRefresh;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Identity.Api.Endpoints.Auth.Shared;

namespace Identity.Api.Endpoints.Auth.MobileRefresh;

internal static class MobileRefreshEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPost("refresh", Handle)
            .WithName("refreshMobile")
            .AllowAnonymous()
            .RequireRateLimiting("AuthRefresh")
            .Produces<MobileTokenResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status429TooManyRequests);

    private static async Task<IResult> Handle(
        [FromBody] MobileRefreshRequest request, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SessionRefreshCommand(new SessionRefreshRequest(request.RefreshToken)), cancellationToken);
        if (result.Status != Ardalis.Result.ResultStatus.Ok || result.Value is null)
            return result.ToProblem(http);

        var value = result.Value;
        return TypedResults.Ok(new MobileTokenResponse(value.TokenType, value.AccessToken, value.ExpiresIn, value.RefreshToken));
    }
}
