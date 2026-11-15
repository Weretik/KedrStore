using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Sales.Application.Abstractions.Orders;
using Sales.Application.Features.OrderWorkspace.Queries.QuoteWorkspaceOrder;
using Sales.Application.Abstractions.Workspace.Pricing;
namespace Sales.Api.Endpoints.OrderWorkspace.QuoteWorkspaceOrder;

internal static class QuoteWorkspaceOrderEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPost("quote", Handle)
            .WithName("quoteAdminWorkspaceOrder")
            .WithSummary("Price a selected customer's lines without persisting an order")
            .Produces<WorkspaceQuote>();

    private static async Task<IResult> Handle(
        [FromBody] WorkspaceQuoteRequest request, ISender sender, HttpContext http, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new QuoteWorkspaceOrderQuery(request), cancellationToken);
        return result.ToHttpResult(http);
    }
}
