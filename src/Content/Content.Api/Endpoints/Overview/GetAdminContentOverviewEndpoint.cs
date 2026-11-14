using Content.Application.Features.Overview.Queries.GetAdminContentOverview;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Content.Api.Endpoints.Overview;

internal static class GetAdminContentOverviewEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("", Handle)
            .WithName("getAdminContentOverview")
            .Produces<AdminContentOverview>();

    private static async Task<IResult> Handle(ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAdminContentOverviewQuery(), cancellationToken);
        return result.ToHttpResult(http);
    }
}
