using Identity.Api.Contracts.Auth;
using Identity.Application.Features.Auth.SessionLogin;
using Identity.Application.Features.Auth.SessionLogout;
using Identity.Application.Features.Auth.SessionMe;
using Identity.Application.Features.Auth.SessionRefresh;
using Identity.Application.Features.Auth.SessionRefresh.DTOs;
using Identity.Api.Options;
using Identity.Application.Features.Auth.SessionLogin.DTOs;

namespace Identity.Api.Controllers;

[ApiController]
[Route("api/auth/session")]
public sealed class AuthSessionController(
    ISender sender,
    IOptions<SessionCookieOptions> sessionCookieOptions) : ControllerBase
{
    private readonly SessionCookieOptions _cookieOptions = sessionCookieOptions.Value;

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

        AppendRefreshCookie(result.Value.RefreshToken);
        AppendCsrfCookie();
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
        if (!IsValidCsrfRequest())
        {
            return BadRequest("Invalid CSRF token.");
        }

        if (!Request.Cookies.TryGetValue(_cookieOptions.RefreshTokenCookieName, out var refreshToken) || string.IsNullOrWhiteSpace(refreshToken))
        {
            return Unauthorized();
        }

        var command = new SessionRefreshCommand(new SessionRefreshRequest(refreshToken));
        var result = await sender.Send(command, cancellationToken);
        if (result.Status != Ardalis.Result.ResultStatus.Ok || result.Value is null)
        {
            return this.ToActionResult(result);
        }

        AppendRefreshCookie(result.Value.RefreshToken);
        AppendCsrfCookie();
        return Ok(new SessionTokenResponse(result.Value.TokenType, result.Value.AccessToken, result.Value.ExpiresIn));
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SessionLogoutCommand(), cancellationToken);
        if (result.Status != Ardalis.Result.ResultStatus.Ok)
        {
            return this.ToActionResult(result);
        }

        DeleteRefreshCookie();
        DeleteCsrfCookie();
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

        var response = new SessionMeResponse(result.Value.UserId, result.Value.Email, result.Value.Roles);
        return Ok(response);
    }

    private void AppendRefreshCookie(string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = _cookieOptions.Secure,
            SameSite = _cookieOptions.SameSite,
            Path = _cookieOptions.RefreshTokenCookiePath,
            MaxAge = TimeSpan.FromDays(_cookieOptions.RefreshTokenCookieTtlDays),
            IsEssential = true
        };

        Response.Cookies.Append(_cookieOptions.RefreshTokenCookieName, refreshToken, cookieOptions);
    }

    private void AppendCsrfCookie()
    {
        var csrfToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        var cookieOptions = new CookieOptions
        {
            HttpOnly = false,
            Secure = _cookieOptions.Secure,
            SameSite = _cookieOptions.SameSite,
            Path = _cookieOptions.CsrfCookiePath,
            MaxAge = TimeSpan.FromDays(_cookieOptions.CsrfCookieTtlDays),
            IsEssential = true
        };

        Response.Cookies.Append(_cookieOptions.CsrfCookieName, csrfToken, cookieOptions);
    }

    private void DeleteRefreshCookie()
    {
        Response.Cookies.Delete(_cookieOptions.RefreshTokenCookieName, new CookieOptions
        {
            Path = _cookieOptions.RefreshTokenCookiePath,
            Secure = _cookieOptions.Secure,
            SameSite = _cookieOptions.SameSite,
            HttpOnly = true,
            IsEssential = true
        });
    }

    private void DeleteCsrfCookie()
    {
        Response.Cookies.Delete(_cookieOptions.CsrfCookieName, new CookieOptions
        {
            Path = _cookieOptions.CsrfCookiePath,
            Secure = _cookieOptions.Secure,
            SameSite = _cookieOptions.SameSite,
            HttpOnly = false,
            IsEssential = true
        });
    }

    private bool IsValidCsrfRequest()
    {
        if (!Request.Cookies.TryGetValue(_cookieOptions.CsrfCookieName, out var csrfCookie) || string.IsNullOrWhiteSpace(csrfCookie))
        {
            return false;
        }

        if (!Request.Headers.TryGetValue(_cookieOptions.CsrfHeaderName, out var csrfHeader) || string.IsNullOrWhiteSpace(csrfHeader))
        {
            return false;
        }

        return string.Equals(csrfCookie, csrfHeader.ToString(), StringComparison.Ordinal);
    }

}
