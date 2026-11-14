using Content.Application.Features.Overview.Queries.GetAdminContentOverview;
using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Content.Api.Features.Overview;

/// <summary>Site content problems shown on the admin «Огляд».</summary>
[ApiController]
[Route("api/admin/content/overview")]
[Authorize(Policy = PolicyNames.CanManageContent)]
public sealed class AdminContentOverviewController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getAdminContentOverview")]
    [ProducesResponseType(typeof(AdminContentOverview), StatusCodes.Status200OK)]
    public async Task<ActionResult<AdminContentOverview>> Get(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAdminContentOverviewQuery(), cancellationToken);
        return this.ToActionResult(result);
    }
}
