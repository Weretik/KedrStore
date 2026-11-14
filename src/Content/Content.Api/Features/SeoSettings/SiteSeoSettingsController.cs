using Content.Application.Features.SeoSettings.Queries.GetSiteSeoSettings;
using Microsoft.AspNetCore.Authorization;

namespace Content.Api.Features.SeoSettings;

/// <summary>Public SEO codes and the link-preview picture that every site page carries.</summary>
[ApiController]
[Route("api/content/seo-settings")]
[AllowAnonymous]
public sealed class SiteSeoSettingsController(ISender sender) : ControllerBase
{
    [HttpGet(Name = "getSiteSeoSettings")]
    [ProducesResponseType(typeof(SiteSeoSettingsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SiteSeoSettingsResponse>> Get(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetSiteSeoSettingsQuery(), cancellationToken);
        return this.ToActionResult(result.Map(SiteSeoSettingsResponseMapper.ToResponse));
    }
}
