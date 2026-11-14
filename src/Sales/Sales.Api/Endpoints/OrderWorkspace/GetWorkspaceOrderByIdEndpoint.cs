using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Sales.Application.Features.OrderWorkspace.Queries.GetWorkspaceOrder;

namespace Sales.Api.Endpoints.OrderWorkspace;

internal static class GetWorkspaceOrderByIdEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapGet("orders/{orderId:long}", Handle)
            .WithName("getAdminWorkspaceOrderById");

    private static async Task<IResult> Handle(
        long orderId, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetWorkspaceOrderQuery(orderId), cancellationToken);
        return result.ToHttpResult(http);
    }
}
