using Identity.Api.Contracts.Auth;
using Identity.Api.Cookies;
using Identity.Application.Features.Auth.SessionLogin;
using Identity.Application.Features.Auth.SessionLogin.DTOs;
using Identity.Application.Features.Auth.SessionLogout;
using Identity.Application.Features.Auth.SessionMe;
using Identity.Application.Features.Auth.SessionRefresh;
using Identity.Application.Features.Auth.SessionRefresh.DTOs;

namespace Identity.Api.Controllers;

/// <summary>
/// Current user for every client, plus the old token-in-JSON flow kept until the frontends move to
/// <see cref="CookieAuthController"/> and <see cref="MobileAuthController"/>.
/// </summary>
[ApiController]
[Route("api/auth/session")]
public sealed class AuthSessionController(ISender sender, SessionCookieWriter cookies) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthLogin")]
    [ProducesResponseType(typeof(SessionTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SessionTokenResponse>> Login([FromBody] SessionLoginRequest request, CancellationToken cancellationToken)
    {
        var command = new SessionLoginCommand(new SessionLoginCommandRequest(request.Email, request.Password));
        var result = await sender.Send(command, cancellationToken);

        if (result.Status != Ardalis.Result.ResultStatus.Ok || result.Value is null)
        {
            return this.ToActionResult(result);
        }

        cookies.AppendRefreshToken(Response, result.Value.RefreshToken);
        cookies.AppendCsrfToken(Response);
        return Ok(new SessionTokenResponse(result.Value.TokenType, result.Value.AccessToken, result.Value.ExpiresIn));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthRefresh")]
    [ProducesResponseType(typeof(SessionTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<SessionTokenResponse>> Refresh(CancellationToken cancellationToken)
    {
        if (!cookies.HasValidCsrfToken(Request))
        {
            return BadRequest("Invalid CSRF token.");
        }

        var refreshToken = cookies.ReadRefreshToken(Request);
        if (refreshToken is null)
        {
            return Unauthorized();
        }

        var result = await sender.Send(new SessionRefreshCommand(new SessionRefreshRequest(refreshToken)), cancellationToken);
        if (result.Status != Ardalis.Result.ResultStatus.Ok || result.Value is null)
        {
            return this.ToActionResult(result);
        }

        cookies.AppendRefreshToken(Response, result.Value.RefreshToken);
        cookies.AppendCsrfToken(Response);
        return Ok(new SessionTokenResponse(result.Value.TokenType, result.Value.AccessToken, result.Value.ExpiresIn));
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SessionLogoutCommand(cookies.ReadRefreshToken(Request)), cancellationToken);
        if (result.Status != Ardalis.Result.ResultStatus.Ok)
        {
            return this.ToActionResult(result);
        }

        cookies.DeleteRefreshToken(Response);
        cookies.DeleteCsrfToken(Response);
        return NoContent();
    }

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
