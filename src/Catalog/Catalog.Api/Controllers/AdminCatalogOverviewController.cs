using Catalog.Application.Features.Overview;
using Catalog.Contracts.Overview;
using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Catalog.Api.Controllers;

/// <summary>Catalog problems shown on the admin «Огляд».</summary>
[ApiController]
[Route("api/admin/catalog/overview")]
[Authorize(Policy = PolicyNames.CanManageProducts)]
public sealed class AdminCatalogOverviewController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getAdminCatalogOverview")]
    [ProducesResponseType(typeof(AdminCatalogOverviewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AdminCatalogOverviewResponse>> Get(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAdminCatalogOverviewQuery(), cancellationToken);
        return this.ToActionResult(result);
    }
}
