using Identity.Application.Features.Auth.SessionLogin.DTOs;
using Identity.Application.Features.Auth.SessionMe.DTOs;
using Identity.Application.Features.Auth.SessionRefresh.DTOs;
using Identity.Application.Services;
using Identity.Infrastructure.Entities;

namespace Identity.Infrastructure.Services;

public sealed class IdentitySessionService(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    IHttpContextAccessor httpContextAccessor,
    IOptionsMonitor<BearerTokenOptions> bearerTokenOptionsMonitor) : IIdentitySessionService
{
    public async Task<SessionLoginResult?> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return null;
        }

        var signInResult = await signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
        if (!signInResult.Succeeded)
        {
            return null;
        }

        var principal = await signInManager.CreateUserPrincipalAsync(user);
        var tokenPayload = await IssueTokenPayloadAsync(principal, cancellationToken);
        if (tokenPayload is null)
        {
            return null;
        }

        return new SessionLoginResult(
            TokenType: tokenPayload.TokenType,
            AccessToken: tokenPayload.AccessToken,
            ExpiresIn: tokenPayload.ExpiresIn,
            RefreshToken: tokenPayload.RefreshToken);
    }

    public async Task<SessionRefreshResult?> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return null;
        }

        var options = bearerTokenOptionsMonitor.Get(IdentityConstants.BearerScheme);
        var refreshTicket = options.RefreshTokenProtector.Unprotect(refreshToken);

        var expiresUtc = refreshTicket?.Properties?.ExpiresUtc;
        if (!expiresUtc.HasValue || expiresUtc.Value <= DateTimeOffset.UtcNow)
        {
            return null;
        }

        var userId = refreshTicket?.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user is null || !await signInManager.CanSignInAsync(user))
        {
            return null;
        }

        var principal = await signInManager.CreateUserPrincipalAsync(user);
        var tokenPayload = await IssueTokenPayloadAsync(principal, cancellationToken);
        if (tokenPayload is null)
        {
            return null;
        }

        return new SessionRefreshResult(
            TokenType: tokenPayload.TokenType,
            AccessToken: tokenPayload.AccessToken,
            ExpiresIn: tokenPayload.ExpiresIn,
            RefreshToken: tokenPayload.RefreshToken);
    }

    public async Task<SessionMeResult?> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await GetCurrentUserEntityAsync(cancellationToken);
        if (user is null)
        {
            return null;
        }

        var roles = await userManager.GetRolesAsync(user);

        return new SessionMeResult(
            UserId: user.Id.ToString(),
            Email: user.Email ?? string.Empty,
            Roles: roles.ToArray());
    }

    public async Task<bool> LogoutAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await GetCurrentUserEntityAsync(cancellationToken);
        if (user is null)
        {
            return false;
        }

        // Revokes existing refresh tokens tied to prior security stamp.
        await userManager.UpdateSecurityStampAsync(user);
        return true;
    }

    private async Task<IdentityBearerTokenPayload?> IssueTokenPayloadAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken)
    {
        var httpContext = httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            return null;
        }

        await using var buffer = new MemoryStream();
        var originalBody = httpContext.Response.Body;
        httpContext.Response.Body = buffer;

        try
        {
            await httpContext.SignInAsync(IdentityConstants.BearerScheme, principal);
            buffer.Position = 0;

            return await JsonSerializer.DeserializeAsync<IdentityBearerTokenPayload>(
                buffer,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                cancellationToken);
        }
        finally
        {
            httpContext.Response.Body = originalBody;
        }
    }

    private sealed record IdentityBearerTokenPayload(
        string TokenType,
        string AccessToken,
        int ExpiresIn,
        string RefreshToken);

    private async Task<AppUser?> GetCurrentUserEntityAsync(CancellationToken cancellationToken)
    {
        var httpContext = httpContextAccessor.HttpContext;
        var userId = httpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user is null || !await signInManager.CanSignInAsync(user))
        {
            return null;
        }

        return user;
    }
}
