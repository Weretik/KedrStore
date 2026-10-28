using Identity.Api.Contracts.Auth;
using Identity.Application.Features.Auth.SessionMe;

namespace Identity.Api.Controllers;

/// <summary>
/// Current user for every client: the browser session cookie or the mobile bearer token.
/// Sign-in lives in <see cref="CookieAuthController"/> and <see cref="MobileAuthController"/>.
/// </summary>
[ApiController]
[Route("api/auth/session")]
public sealed class AuthSessionController(ISender sender) : ControllerBase
{
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(SessionMeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SessionMeResponse>> Me(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SessionMeQuery(), cancellationToken);
        if (result.Status != Ardalis.Result.ResultStatus.Ok || result.Value is null)
        {
            return this.ToActionResult(result);
        }

        return Ok(SessionMeResponse.From(result.Value));
    }
}
