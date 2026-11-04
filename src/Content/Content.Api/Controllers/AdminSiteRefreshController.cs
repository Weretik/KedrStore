using Content.Api.Site;
using Content.Application.Contracts.Site;
using Content.Application.Features.Site.Refresh;
using Identity.Application.Security.Policies;
using Microsoft.AspNetCore.Authorization;

namespace Content.Api.Controllers;

/// <summary>«Оновити сайт зараз»: asks the public site to drop its cached content and catalog.</summary>
[ApiController]
[Route("api/admin/content/site-refresh")]
[Authorize(Policy = PolicyNames.CanManageContent)]
public sealed class AdminSiteRefreshController(ISender sender) : ControllerBase
{
    [HttpPost(Name = "refreshSite")]
    [ProducesResponseType(typeof(SiteRefreshResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(SiteRefreshResponse), StatusCodes.Status502BadGateway)]
    [ProducesResponseType(typeof(SiteRefreshResponse), StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult<SiteRefreshResponse>> Refresh(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RefreshSiteCommand(), cancellationToken);
        if (!result.IsSuccess)
            return this.ToActionResult(result.Map(outcome => new SiteRefreshResponse(outcome.ToString())));
        return result.Value switch
        {
            SiteRefreshOutcome.Refreshed => Ok(new SiteRefreshResponse("refreshed")),
            SiteRefreshOutcome.NotConfigured => StatusCode(StatusCodes.Status503ServiceUnavailable,
                new SiteRefreshResponse("notConfigured")),
            _ => StatusCode(StatusCodes.Status502BadGateway, new SiteRefreshResponse("siteUnavailable")),
        };
    }
}
