using Content.Application.Features.Wholesale.Queries.GetPublicWholesalePage;
using Microsoft.AspNetCore.Authorization;

namespace Content.Api.Features.Wholesale;

/// <summary>The texts of the site page «Співпраця».</summary>
[ApiController]
[Route("api/content")]
[AllowAnonymous]
public sealed class WholesalePageController(ISender sender) : ControllerBase
{
    [HttpGet("{lang}/wholesale", Name = "getPublicWholesalePage")]
    [ProducesResponseType(typeof(PublicWholesalePageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PublicWholesalePageResponse>> Get([FromRoute] string lang,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPublicWholesalePageQuery(lang), cancellationToken);
        return this.ToActionResult(result.Map(WholesaleResponseMapper.ToResponse));
    }
}
