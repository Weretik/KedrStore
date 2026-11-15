using Sales.Application.Abstractions.Workspace.Orders;
using Sales.Application.Abstractions.Workspace;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Sales.Application.Features.OrderWorkspace.Queries.GetWorkspaceOrders;
namespace Sales.Api.Endpoints.OrderWorkspace.GetWorkspaceOrderList;

internal static class GetWorkspaceOrderListEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("orders", Handle)
            .WithName("getAdminWorkspaceOrders")
            .WithSummary("Search persisted Admin orders")
            .Produces<WorkspacePage<WorkspaceOrderListItem>>();

    private static async Task<IResult> Handle(
        ISender sender, HttpContext http,
        [FromQuery] string? searchTerm, [FromQuery] string? counterpartyId,
        [FromQuery] string? createdFrom, [FromQuery] string? createdTo,
        CancellationToken cancellationToken,
        [FromQuery] string sort = "-createdAt", [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await sender.Send(new GetWorkspaceOrdersQuery(searchTerm, counterpartyId,
            createdFrom, createdTo, sort, page, pageSize), cancellationToken);
        return result.ToHttpResult(http);
    }
}
