using Content.Application.Abstractions.Integrations;
using Content.Application.Features.Site.Commands.RefreshSite;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Content.Api.Endpoints.Site;

internal static class RefreshSiteEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPost("", Handle)
            .WithName("refreshSite")
            .Produces<SiteRefreshResponse>()
            .Produces<SiteRefreshResponse>(StatusCodes.Status502BadGateway)
            .Produces<SiteRefreshResponse>(StatusCodes.Status503ServiceUnavailable);

    private static async Task<IResult> Handle(ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RefreshSiteCommand(), cancellationToken);
        if (!result.IsSuccess)
            return (result.Map(outcome => new SiteRefreshResponse(outcome.ToString()))).ToHttpResult(http);
        return result.Value switch
        {
            SiteRefreshOutcome.Refreshed => TypedResults.Ok(new SiteRefreshResponse("refreshed")),
            SiteRefreshOutcome.NotConfigured => TypedResults.Json(new SiteRefreshResponse("notConfigured"),
                statusCode: StatusCodes.Status503ServiceUnavailable),
            _ => TypedResults.Json(new SiteRefreshResponse("siteUnavailable"),
                statusCode: StatusCodes.Status502BadGateway),
        };
    }
}
