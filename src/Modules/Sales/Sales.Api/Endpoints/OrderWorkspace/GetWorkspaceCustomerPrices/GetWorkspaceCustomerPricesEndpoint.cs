using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Sales.Application.Abstractions.Orders;
using Sales.Application.Features.OrderWorkspace.Queries.GetWorkspaceCustomerPrices;
using Sales.Application.Abstractions.Workspace.Pricing;
namespace Sales.Api.Endpoints.OrderWorkspace.GetWorkspaceCustomerPrices;

internal static class GetWorkspaceCustomerPricesEndpoint
{
    public static RouteHandlerBuilder Map(RouteGroupBuilder group) =>
        group.MapPost("customer-prices", Handle)
            .WithName("getAdminWorkspaceCustomerPrices")
            .WithSummary("Read a customer's unit prices for products, ignoring stock")
            .WithDescription("Uses the price type of the nearest customer rule on the product's category or any parent category, otherwise the customer's default price type. A product without such a UAH price, or an unknown product, returns a null unitPrice.")
            .Produces<WorkspaceCustomerPrices>();

    private static async Task<IResult> Handle(
        [FromBody] WorkspaceCustomerPricesRequest request, ISender sender, HttpContext http,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetWorkspaceCustomerPricesQuery(request), cancellationToken);
        return result.ToHttpResult(http);
    }
}
