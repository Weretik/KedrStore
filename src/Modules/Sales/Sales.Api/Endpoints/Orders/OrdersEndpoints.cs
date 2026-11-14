using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Sales.Api.Endpoints.Orders.GetOrderById;

using Sales.Api.Endpoints.Orders.RetryOrderSync;
using Sales.Api.Endpoints.Orders.GetOrderList;
using Sales.Api.Endpoints.Orders.CreateOrder;
using Sales.Api.Endpoints.Orders.GetOrderSyncStatus;

namespace Sales.Api.Endpoints.Orders;

/// <summary>Orders for the admin: create with an idempotency key, read, and the 1C sync status and retry.</summary>
public static class OrdersEndpoints
{
    public static IEndpointRouteBuilder MapOrdersEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var admin = endpoints.MapGroup("api/admin/orders")
            .WithTags("AdminOrders")
            .RequireAuthorization(PolicyNames.CanManageOrders);
        CreateOrderEndpoint.Map(admin);
        GetOrderSyncStatusEndpoint.Map(admin);
        RetryOrderSyncEndpoint.Map(admin);
        GetOrderListEndpoint.Map(admin);
        GetOrderByIdEndpoint.Map(admin);

        return endpoints;
    }
}
