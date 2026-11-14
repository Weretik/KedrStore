using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Catalog.Api.Endpoints.MobileSync.GetMobileCatalogSnapshot;
namespace Catalog.Api.Endpoints.MobileSync;

/// <summary>The catalog snapshot the offline manager app keeps on the device (spec platform/004).</summary>
public static class MobileSyncEndpoints
{
    public static IEndpointRouteBuilder MapMobileSyncEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var mobileSyncCatalog = endpoints.MapGroup("api/mobile/sync/catalog")
            .WithTags("MobileSyncCatalog")
            .RequireAuthorization(PolicyNames.CanManageOrders);
        GetMobileCatalogSnapshotEndpoint.Map(mobileSyncCatalog);

        return endpoints;
    }
}
