using Identity.Api.Contracts.Auth;
using Identity.Application.Features.Auth.SessionLogin;
using Identity.Application.Features.Auth.SessionLogin.DTOs;
using Identity.Application.Features.Auth.SessionLogout;
using Identity.Application.Features.Auth.SessionRefresh;
using Identity.Application.Features.Auth.SessionRefresh.DTOs;

namespace Identity.Api.Controllers;

/// <summary>
/// Mobile sign-in: access and refresh tokens in JSON; the app keeps them in the device secure store.
/// </summary>
[ApiController]
[Route("api/auth/mobile")]
public sealed class MobileAuthController(ISender sender) : ControllerBase
{
    [HttpPost("login", Name = "loginMobile")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthLogin")]
    [ProducesResponseType(typeof(MobileTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<MobileTokenResponse>> Login([FromBody] SessionLoginRequest request, CancellationToken cancellationToken)
    {
        var command = new SessionLoginCommand(new SessionLoginCommandRequest(request.Email, request.Password));
        var result = await sender.Send(command, cancellationToken);
        if (result.Status != Ardalis.Result.ResultStatus.Ok || result.Value is null)
        {
            return this.ToActionResult(result);
        }

        var value = result.Value;
        return Ok(new MobileTokenResponse(value.TokenType, value.AccessToken, value.ExpiresIn, value.RefreshToken));
    }

    [HttpPost("refresh", Name = "refreshMobile")]
    [AllowAnonymous]
    [EnableRateLimiting("AuthRefresh")]
    [ProducesResponseType(typeof(MobileTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<MobileTokenResponse>> Refresh([FromBody] MobileRefreshRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SessionRefreshCommand(new SessionRefreshRequest(request.RefreshToken)), cancellationToken);
        if (result.Status != Ardalis.Result.ResultStatus.Ok || result.Value is null)
        {
            return this.ToActionResult(result);
        }

        var value = result.Value;
        return Ok(new MobileTokenResponse(value.TokenType, value.AccessToken, value.ExpiresIn, value.RefreshToken));
    }

    [HttpPost("logout", Name = "logoutMobile")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout([FromBody] MobileLogoutRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SessionLogoutCommand(request.RefreshToken), cancellationToken);
        return result.Status == Ardalis.Result.ResultStatus.Ok ? NoContent() : this.ToActionResult(result);
    }
}
