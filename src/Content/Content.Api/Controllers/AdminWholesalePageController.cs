using Content.Api.Wholesale;
using Content.Application.Features.Wholesale.Admin.Get;
using Content.Application.Features.Wholesale.Admin.Update;
using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Content.Api.Controllers;

/// <summary>Admin editing of the site page «Співпраця» in both languages.</summary>
[ApiController]
[Route("api/admin/content/wholesale")]
[Authorize(Policy = PolicyNames.CanManageContent)]
public sealed class AdminWholesalePageController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getAdminWholesalePage")]
    [ProducesResponseType(typeof(AdminWholesalePageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminWholesalePageResponse>> Get(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAdminWholesalePageQuery(), cancellationToken);
        return this.ToActionResult(result.Map(WholesaleResponseMapper.ToResponse));
    }

    [HttpPut(Name = "updateAdminWholesalePage")]
    [ProducesResponseType(typeof(AdminWholesalePageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminWholesalePageResponse>> Update(
        [FromBody] SaveWholesalePageRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateWholesalePageCommand(WholesaleResponseMapper.ToInput(request)),
            cancellationToken);
        return this.ToActionResult(result.Map(WholesaleResponseMapper.ToResponse));
    }
}
