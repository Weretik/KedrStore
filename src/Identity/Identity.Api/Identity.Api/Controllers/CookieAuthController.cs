using Identity.Api.Contracts.Auth;
using Identity.Api.Cookies;
using Identity.Application.Features.Auth.CookieLogin;
using Identity.Application.Features.Auth.CookieLogout;

namespace Identity.Api.Controllers;

/// <summary>
/// Browser sign-in for Admin and the storefront: an HttpOnly session cookie plus a readable CSRF cookie.
/// </summary>
[ApiController]
[Route("api/auth/cookie")]
public sealed class CookieAuthController(ISender sender, SessionCookieWriter cookies) : ControllerBase
{
    [HttpPost("login", Name = "loginCookieSession")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthLogin")]
    [ProducesResponseType(typeof(SessionMeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SessionMeResponse>> Login([FromBody] SessionLoginRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CookieLoginCommand(request.Email, request.Password), cancellationToken);
        if (result.Status != Ardalis.Result.ResultStatus.Ok || result.Value is null)
        {
            return this.ToActionResult(result);
        }

        cookies.AppendCsrfToken(Response);
        return Ok(SessionMeResponse.From(result.Value));
    }

    [HttpPost("logout", Name = "logoutCookieSession")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        await sender.Send(new CookieLogoutCommand(), cancellationToken);
        cookies.DeleteCsrfToken(Response);
        return NoContent();
    }
}
