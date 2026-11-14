using Content.Application.Features.SeoSettings.Queries.GetSiteSeoSettings;
using Content.Application.Features.SeoSettings.Commands.UpdateSiteSeoSettings;
using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Content.Api.Features.SeoSettings;

/// <summary>«Налаштування SEO сайту»: analytics ids, search console verification codes, link-preview picture.</summary>
[ApiController]
[Route("api/admin/content/seo-settings")]
[Authorize(Policy = PolicyNames.CanManageContent)]
public sealed class AdminSiteSeoSettingsController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getAdminSiteSeoSettings")]
    [ProducesResponseType(typeof(AdminSiteSeoSettingsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminSiteSeoSettingsResponse>> Get(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetSiteSeoSettingsQuery(), cancellationToken);
        return this.ToActionResult(result.Map(SiteSeoSettingsResponseMapper.ToAdminResponse));
    }

    [HttpPut(Name = "updateAdminSiteSeoSettings")]
    [ProducesResponseType(typeof(AdminSiteSeoSettingsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminSiteSeoSettingsResponse>> Update(
        [FromBody] SaveSiteSeoSettingsRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new UpdateSiteSeoSettingsCommand(SiteSeoSettingsResponseMapper.ToInput(request)), cancellationToken);
        return this.ToActionResult(result.Map(SiteSeoSettingsResponseMapper.ToAdminResponse));
    }
}
