using BuildingBlocks.Api.Snapshots;
using Identity.Contracts.Authorization;
using Sales.Application.Features.MobileSync.Queries.GetCustomersSnapshot;

namespace Sales.Api.Features.MobileSync;

/// <summary>The customers snapshot the offline manager app keeps on the device (spec platform/004).</summary>
[ApiController]
[Route("api/mobile/sync/customers")]
[Authorize(Policy = PolicyNames.CanManageOrders)]
public sealed class MobileSyncCustomersController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getMobileCustomersSnapshot")]
    [ProducesResponseType(typeof(CustomersSnapshotDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCustomersSnapshotQuery(), cancellationToken);
        return this.ToSnapshotResult(result, snapshot => snapshot.Version);
    }
}
