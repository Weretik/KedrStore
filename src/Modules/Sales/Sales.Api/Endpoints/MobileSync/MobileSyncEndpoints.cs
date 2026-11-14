using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Sales.Api.Endpoints.MobileSync;

/// <summary>The customers snapshot the offline manager app keeps on the device (spec platform/004).</summary>
public static class MobileSyncEndpoints
{
    public static IEndpointRouteBuilder MapMobileSyncEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var mobile = endpoints.MapGroup("api/mobile/sync/customers")
            .WithTags("MobileSyncCustomers")
            .RequireAuthorization(PolicyNames.CanManageOrders);
        GetCustomersSnapshotEndpoint.Map(mobile);

        return endpoints;
    }
}
