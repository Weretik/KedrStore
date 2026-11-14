using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Api.Endpoints.Orders;

/// <summary>The site quick order: an anonymous, rate-limited order without an account.</summary>
public static class OrdersEndpoints
{
    public static IEndpointRouteBuilder MapOrdersEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var order = endpoints.MapGroup("api/orders")
            .WithTags("Order");
        CreateOrderEndpoint.Map(order);

        return endpoints;
    }
}
