using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Sales.Application.Abstractions.Orders;
using Sales.Application.Features.OrderWorkspace.Queries.GetWorkspaceOverview;
using Sales.Application.Features.OrderWorkspace.Shared;
namespace Sales.Api.Endpoints.OrderWorkspace.GetWorkspaceOverview;

internal static class GetWorkspaceOverviewEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("overview", Handle)
            .WithName("getAdminWorkspaceOverview")
            .Produces<WorkspaceOverview>();

    /// <summary>Counts and short lists for the admin «Огляд»; «today» is the current Kyiv day.</summary>
    private static async Task<IResult> Handle(
        [FromServices] TimeProvider clock, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var todayFrom = WorkspaceDateRange.TodayStartUtc(clock.GetUtcNow());
        var result = await sender.Send(new GetWorkspaceOverviewQuery(todayFrom), cancellationToken);
        return result.ToHttpResult(http);
    }
}
