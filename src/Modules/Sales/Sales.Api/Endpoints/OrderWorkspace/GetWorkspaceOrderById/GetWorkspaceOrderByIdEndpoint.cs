using Sales.Application.Abstractions.Workspace.Orders;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Sales.Application.Features.OrderWorkspace.Queries.GetWorkspaceOrder;
namespace Sales.Api.Endpoints.OrderWorkspace.GetWorkspaceOrderById;

internal static class GetWorkspaceOrderByIdEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("orders/{orderId:long}", Handle)
            .WithName("getAdminWorkspaceOrderById")
            .WithSummary("Read a persisted Admin order")
            .Produces<WorkspaceOrderDetail>();

    private static async Task<IResult> Handle(
        long orderId, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetWorkspaceOrderQuery(orderId), cancellationToken);
        return result.ToHttpResult(http);
    }
}
