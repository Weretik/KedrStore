using Content.Application.Features.SiteSeo.Queries.ListPublicPageSeo;
using Microsoft.AspNetCore.Authorization;

namespace Content.Api.Features.SiteSeo;

/// <summary>Editor-set search titles and descriptions of the site pages; the site keeps its own for the rest.</summary>
[ApiController]
[Route("api/content")]
[AllowAnonymous]
public sealed class PageSeoController(ISender sender) : ControllerBase
{
    [HttpGet("{lang}/page-seo", Name = "listPublicPageSeo")]
    [ProducesResponseType(typeof(IReadOnlyList<PublicPageSeoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyList<PublicPageSeoResponse>>> List([FromRoute] string lang,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ListPublicPageSeoQuery(lang), cancellationToken);
        return this.ToActionResult(result.Map(PageSeoResponseMapper.ToResponse));
    }
}
