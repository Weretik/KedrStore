using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Sales.Api.Endpoints.QuickOrders.GetQuickOrderById;
using Sales.Api.Endpoints.QuickOrders.GetQuickOrderList;
using Sales.Api.Endpoints.QuickOrders.PlaceQuickOrder;
using Sales.Api.Endpoints.QuickOrders.ResendQuickOrderToTelegram;
namespace Sales.Api.Endpoints.QuickOrders;

/// <summary>
/// The site quick order: an anonymous, rate-limited order without an account (platform spec 006, R-007), and its Admin
/// list, detail and Telegram resend (spec sales/009).
/// </summary>
public static class QuickOrdersEndpoints
{
    public static IEndpointRouteBuilder MapQuickOrdersEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var order = endpoints.MapGroup("api/orders")
            .WithTags("Order");
        PlaceQuickOrderEndpoint.Map(order);

        var admin = endpoints.MapGroup("api/admin/quick-orders")
            .WithTags("AdminQuickOrders")
            .RequireAuthorization(PolicyNames.CanManageOrders);
        GetQuickOrderListEndpoint.Map(admin);
        GetQuickOrderByIdEndpoint.Map(admin);
        ResendQuickOrderToTelegramEndpoint.Map(admin);

        return endpoints;
    }
}
