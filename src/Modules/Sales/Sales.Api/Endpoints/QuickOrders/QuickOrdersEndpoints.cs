using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Sales.Api.Endpoints.QuickOrders;

/// <summary>The site quick order: an anonymous, rate-limited order without an account (platform spec 006, R-007).</summary>
public static class QuickOrdersEndpoints
{
    public static IEndpointRouteBuilder MapQuickOrdersEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var order = endpoints.MapGroup("api/orders")
            .WithTags("Order");
        PlaceQuickOrderEndpoint.Map(order);

        return endpoints;
    }
}
