using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Sales.Api.Endpoints.OrderWorkspace;

/// <summary>The admin order workspace: overview, orders, quotes, customer prices, checkout and drafts.</summary>
public static class OrderWorkspaceEndpoints
{
    public static IEndpointRouteBuilder MapOrderWorkspaceEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var admin = endpoints.MapGroup("api/admin/order-workspace")
            .WithTags("AdminOrderWorkspace")
            .RequireAuthorization(PolicyNames.CanManageOrders);
        GetWorkspaceOverviewEndpoint.Map(admin);
        GetWorkspaceOrderListEndpoint.Map(admin);
        GetWorkspaceOrderByIdEndpoint.Map(admin);
        QuoteWorkspaceOrderEndpoint.Map(admin);
        GetWorkspaceCustomerPricesEndpoint.Map(admin);
        CreateWorkspaceOrderEndpoint.Map(admin);
        GetWorkspaceDraftListEndpoint.Map(admin);
        GetWorkspaceDraftByIdEndpoint.Map(admin);
        CreateWorkspaceDraftEndpoint.Map(admin);
        UpdateWorkspaceDraftEndpoint.Map(admin);
        DeleteWorkspaceDraftEndpoint.Map(admin);

        return endpoints;
    }
}
