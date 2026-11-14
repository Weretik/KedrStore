using Identity.Application.Features.Auth.Queries.SessionMe;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Identity.Api.Endpoints.Auth.Shared;

namespace Identity.Api.Endpoints.Auth.GetSessionMe;

internal static class GetSessionMeEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("me", Handle)
            .Produces<SessionMeResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized);

    private static async Task<IResult> Handle(ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SessionMeQuery(), cancellationToken);
        return result.Status == Ardalis.Result.ResultStatus.Ok && result.Value is not null
            ? TypedResults.Ok(SessionMeResponse.From(result.Value))
            : result.ToProblem(http);
    }
}
