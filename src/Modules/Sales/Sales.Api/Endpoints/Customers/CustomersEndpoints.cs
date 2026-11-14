using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Sales.Api.Endpoints.Customers;

/// <summary>Customers (1C counterparties) for the admin.</summary>
public static class CustomersEndpoints
{
    public static IEndpointRouteBuilder MapCustomersEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var admin = endpoints.MapGroup("api/admin/customers")
            .WithTags("AdminCustomers")
            .RequireAuthorization(PolicyNames.CanManageOrders);
        GetCustomerListEndpoint.Map(admin);
        GetCustomerByIdEndpoint.Map(admin);

        return endpoints;
    }
}
