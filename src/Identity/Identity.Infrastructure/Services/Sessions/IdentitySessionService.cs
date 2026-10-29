using Identity.Infrastructure.Avatars;
using Identity.Application.Features.Auth.SessionLogin.DTOs;
using Identity.Application.Features.Auth.SessionMe.DTOs;
using Identity.Application.Features.Auth.SessionRefresh.DTOs;
using Identity.Application.Services;
using Identity.Infrastructure.Entities;
using Identity.Infrastructure.Options;

namespace Identity.Infrastructure.Services.Sessions;

public sealed class IdentitySessionService(
    RefreshSessionStore refreshSessionStore,
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    IdentityBearerTokenIssuer bearerTokenIssuer,
    AvatarUrlBuilder avatarUrlBuilder,
    IHttpContextAccessor httpContextAccessor,
    ILogger<IdentitySessionService> logger,
    IOptions<IdentitySessionPerformanceOptions> performanceOptions,
    IOptions<IdentitySessionSecurityOptions> securityOptions) : IIdentitySessionService
{
    public async Task<SessionLoginResult?> LoginAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("AUDIT: Login failed: empty credentials");
            return null;
        }

        var stepWatch = Stopwatch.StartNew();
        var user = await userManager.FindByEmailAsync(email);
        LogStepDuration("Login.FindByEmail", stepWatch.ElapsedMilliseconds);
        if (user is null)
        {
            logger.LogWarning("AUDIT: Login failed: user not found for email {Email}", email);
            return null;
        }

        stepWatch.Restart();
        var signInResult = await signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
        LogStepDuration("Login.CheckPassword", stepWatch.ElapsedMilliseconds);
        if (!signInResult.Succeeded)
        {
            logger.LogWarning("AUDIT: Login failed: invalid credentials for userId {UserId}", user.Id);
            return null;
        }

        stepWatch.Restart();
        var principal = await signInManager.CreateUserPrincipalAsync(user);
        LogStepDuration("Login.CreatePrincipal", stepWatch.ElapsedMilliseconds);

        var tokenPayload = bearerTokenIssuer.Issue(principal);

        var now = DateTimeOffset.UtcNow;
        var absoluteExpiresAtUtc = now.AddDays(securityOptions.Value.RefreshAbsoluteLifetimeDays);
        await refreshSessionStore.CreateAsync(user.Id, tokenPayload.RefreshToken, now, absoluteExpiresAtUtc, cancellationToken);
        logger.LogInformation("AUDIT: Login success for userId {UserId}", user.Id);

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
            logger.LogWarning("AUDIT: Refresh failed: empty refresh token");
            return null;
        }

        var now = DateTimeOffset.UtcNow;
        var refreshSession = await refreshSessionStore.FindByTokenAsync(refreshToken, cancellationToken);

        if (refreshSession is null)
        {
            logger.LogWarning("AUDIT: Refresh failed: refresh token hash not found");
            return null;
        }

        if (IsWithinRotationGrace(refreshSession, now))
        {
            // A parallel tab or retry presented the token that was just rotated; not a theft signal.
            logger.LogInformation("AUDIT: Refresh rejected: session {SessionId} was rotated moments ago", refreshSession.Id);
            return null;
        }

        if (refreshSession.ReplacedBySessionId.HasValue)
        {
            await HandleRefreshReuseAsync(refreshSession, now, cancellationToken);
            return null;
        }

        if (refreshSession.RevokedAtUtc.HasValue)
        {
            // Logged out or expired; only a rotated token presented again signals theft.
            logger.LogWarning("AUDIT: Refresh failed: session {SessionId} is revoked ({Reason})",
                refreshSession.Id,
                refreshSession.RevocationReason);
            return null;
        }

        if (refreshSession.AbsoluteExpiresAtUtc <= now)
        {
            await refreshSessionStore.RevokeAsync(refreshSession, now, "ExpiredAbsolute", cancellationToken);
            logger.LogWarning("AUDIT: Refresh failed: absolute lifetime exceeded for userId {UserId}", refreshSession.UserId);
            return null;
        }

        var idleExpiresAt = refreshSession.LastUsedAtUtc.AddDays(securityOptions.Value.RefreshIdleTimeoutDays);
        if (idleExpiresAt <= now)
        {
            await refreshSessionStore.RevokeAsync(refreshSession, now, "ExpiredIdle", cancellationToken);
            logger.LogWarning("AUDIT: Refresh failed: idle timeout exceeded for userId {UserId}", refreshSession.UserId);
            return null;
        }

        var stepWatch = Stopwatch.StartNew();
        var user = await userManager.FindByIdAsync(refreshSession.UserId.ToString());
        LogStepDuration("Refresh.FindById", stepWatch.ElapsedMilliseconds);
        stepWatch.Restart();
        if (user is null || !await signInManager.CanSignInAsync(user))
        {
            logger.LogWarning("AUDIT: Refresh failed: user unavailable or sign-in blocked for userId {UserId}", refreshSession.UserId);
            return null;
        }
        LogStepDuration("Refresh.CanSignIn", stepWatch.ElapsedMilliseconds);

        stepWatch.Restart();
        var principal = await signInManager.CreateUserPrincipalAsync(user);
        LogStepDuration("Refresh.CreatePrincipal", stepWatch.ElapsedMilliseconds);
        var tokenPayload = bearerTokenIssuer.Issue(principal);

        var newSession = await refreshSessionStore.RotateAsync(refreshSession, tokenPayload.RefreshToken, now, cancellationToken);
        if (newSession is null)
        {
            logger.LogInformation("AUDIT: Refresh rejected: session {SessionId} was rotated by a parallel request", refreshSession.Id);
            return null;
        }

        logger.LogInformation("AUDIT: Refresh success for userId {UserId}; rotated sessionId {OldSessionId} -> {NewSessionId}",
            refreshSession.UserId,
            refreshSession.Id,
            newSession.Id);

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
            FullName: user.FullName,
            Roles: roles.ToArray(),
            AvatarUrl: avatarUrlBuilder.Build(user.AvatarPath));
    }

    public async Task<bool> LogoutAsync(string? refreshToken, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await GetCurrentUserEntityAsync(cancellationToken);
        if (user is null)
        {
            return false;
        }

        var now = DateTimeOffset.UtcNow;
        var currentSession = string.IsNullOrWhiteSpace(refreshToken)
            ? null
            : await refreshSessionStore.FindByTokenAsync(refreshToken, cancellationToken);

        if (currentSession is not null && currentSession.UserId == user.Id)
        {
            if (!currentSession.RevokedAtUtc.HasValue)
            {
                await refreshSessionStore.RevokeAsync(currentSession, now, "Logout", cancellationToken);
            }

            logger.LogInformation("AUDIT: Logout success for userId {UserId}, sessionId {SessionId}", user.Id, currentSession.Id);
            return true;
        }

        // Without the current refresh session there is nothing narrower to end.
        await refreshSessionStore.RevokeAllActiveByUserAsync(user.Id, now, "Logout", cancellationToken);
        logger.LogInformation("AUDIT: Logout of all sessions for userId {UserId}", user.Id);
        return true;
    }

    private async Task HandleRefreshReuseAsync(
        AppRefreshSession refreshSession,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        refreshSession.ReuseDetectedAtUtc = now;
        refreshSession.RevocationReason ??= "ReuseDetected";
        await refreshSessionStore.RevokeAllActiveByUserAsync(refreshSession.UserId, now, "ReuseDetected", cancellationToken);

        logger.LogWarning("AUDIT: Refresh reuse detected for userId {UserId}, sessionId {SessionId}",
            refreshSession.UserId,
            refreshSession.Id);
    }

    private bool IsWithinRotationGrace(AppRefreshSession refreshSession, DateTimeOffset now) =>
        refreshSession.RevocationReason == RefreshSessionStore.RotatedReason &&
        refreshSession.RevokedAtUtc is { } revokedAt &&
        now - revokedAt < TimeSpan.FromSeconds(securityOptions.Value.RefreshReuseGraceSeconds);

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

    private void LogStepDuration(string step, long elapsedMilliseconds)
    {
        if (elapsedMilliseconds >= performanceOptions.Value.SlowStepThresholdMs &&
            logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Identity session step {Step} took {ElapsedMs} ms", step, elapsedMilliseconds);
        }
    }
}
