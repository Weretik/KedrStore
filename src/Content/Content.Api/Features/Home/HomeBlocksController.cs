using Content.Application.Features.Home.Queries.GetPublicHomeBlocks;
using Microsoft.AspNetCore.Authorization;

namespace Content.Api.Features.Home;

/// <summary>The home page FAQ and brands the site shows.</summary>
[ApiController]
[Route("api/content")]
[AllowAnonymous]
public sealed class HomeBlocksController(ISender sender) : ControllerBase
{
    [HttpGet("{lang}/home", Name = "getPublicHomeBlocks")]
    [ProducesResponseType(typeof(PublicHomeBlocksResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PublicHomeBlocksResponse>> Get([FromRoute] string lang,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPublicHomeBlocksQuery(lang), cancellationToken);
        return this.ToActionResult(result.Map(HomeResponseMapper.ToResponse));
    }
}
