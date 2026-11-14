using BuildingBlocks.Api.Snapshots;
using Catalog.Application.Features.MobileSync.Queries.GetCatalogSnapshot;
using Catalog.Contracts.MobileSync;
using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Catalog.Api.Controllers;

/// <summary>The catalog snapshot the offline manager app keeps on the device (spec platform/004).</summary>
[ApiController]
[Route("api/mobile/sync/catalog")]
[Authorize(Policy = PolicyNames.CanManageOrders)]
public sealed class MobileSyncCatalogController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getMobileCatalogSnapshot")]
    [ProducesResponseType(typeof(CatalogSnapshotResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCatalogSnapshotQuery(), cancellationToken);
        return this.ToSnapshotResult(result, snapshot => snapshot.Version);
    }
}
