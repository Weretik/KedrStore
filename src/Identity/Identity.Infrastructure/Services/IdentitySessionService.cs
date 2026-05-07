using System.Security.Cryptography;
using Identity.Application.Features.Auth.SessionLogin.DTOs;
using Identity.Application.Features.Auth.SessionMe.DTOs;
using Identity.Application.Features.Auth.SessionRefresh.DTOs;
using Identity.Application.Services;
using Identity.Infrastructure.DataBase;
using Identity.Infrastructure.Entities;
using Identity.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;

namespace Identity.Infrastructure.Services;

public sealed class IdentitySessionService(
    AppIdentityDbContext dbContext,
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    IHttpContextAccessor httpContextAccessor,
    IOptionsMonitor<BearerTokenOptions> bearerTokenOptionsMonitor,
    ILogger<IdentitySessionService> logger,
    IOptions<IdentitySessionPerformanceOptions> performanceOptions,
    IOptions<IdentitySessionSecurityOptions> securityOptions) : IIdentitySessionService
{
    private static readonly JsonSerializerOptions TokenJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

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

        stepWatch.Restart();
        var tokenPayload = await IssueTokenPayloadAsync(principal, cancellationToken);
        LogStepDuration("Login.IssueTokenPayload", stepWatch.ElapsedMilliseconds);
        if (tokenPayload is null)
        {
            logger.LogWarning("AUDIT: Login failed: token payload generation returned null for userId {UserId}", user.Id);
            return null;
        }

        var now = DateTimeOffset.UtcNow;
        var absoluteExpiresAtUtc = now.AddDays(securityOptions.Value.RefreshAbsoluteLifetimeDays);
        await CreateRefreshSessionAsync(user.Id, tokenPayload.RefreshToken, now, absoluteExpiresAtUtc, cancellationToken);
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
        var tokenHash = ComputeTokenHash(refreshToken);
        var refreshSession = await dbContext.RefreshSessions
            .SingleOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);

        if (refreshSession is null)
        {
            logger.LogWarning("AUDIT: Refresh failed: refresh token hash not found");
            return null;
        }

        if (refreshSession.RevokedAtUtc.HasValue || refreshSession.ReplacedBySessionId.HasValue)
        {
            await HandleRefreshReuseAsync(refreshSession, now, cancellationToken);
            return null;
        }

        if (refreshSession.AbsoluteExpiresAtUtc <= now)
        {
            await RevokeSessionAsync(refreshSession, now, "ExpiredAbsolute", cancellationToken);
            logger.LogWarning("AUDIT: Refresh failed: absolute lifetime exceeded for userId {UserId}", refreshSession.UserId);
            return null;
        }

        var idleExpiresAt = refreshSession.LastUsedAtUtc.AddDays(securityOptions.Value.RefreshIdleTimeoutDays);
        if (idleExpiresAt <= now)
        {
            await RevokeSessionAsync(refreshSession, now, "ExpiredIdle", cancellationToken);
            logger.LogWarning("AUDIT: Refresh failed: idle timeout exceeded for userId {UserId}", refreshSession.UserId);
            return null;
        }

        var stepWatch = Stopwatch.StartNew();
        var options = bearerTokenOptionsMonitor.Get(IdentityConstants.BearerScheme);
        var refreshTicket = options.RefreshTokenProtector.Unprotect(refreshToken);
        LogStepDuration("Refresh.UnprotectToken", stepWatch.ElapsedMilliseconds);

        var userId = refreshTicket?.Principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userId, out var parsedUserId) || parsedUserId != refreshSession.UserId)
        {
            await RevokeSessionAsync(refreshSession, now, "PrincipalMismatch", cancellationToken);
            logger.LogWarning("AUDIT: Refresh failed: principal mismatch for sessionId {SessionId}", refreshSession.Id);
            return null;
        }

        stepWatch.Restart();
        var user = await userManager.FindByIdAsync(userId);
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
        stepWatch.Restart();
        var tokenPayload = await IssueTokenPayloadAsync(principal, cancellationToken);
        LogStepDuration("Refresh.IssueTokenPayload", stepWatch.ElapsedMilliseconds);
        if (tokenPayload is null)
        {
            logger.LogWarning("AUDIT: Refresh failed: token payload generation returned null for userId {UserId}", refreshSession.UserId);
            return null;
        }

        var newSession = await CreateRefreshSessionAsync(
            refreshSession.UserId,
            tokenPayload.RefreshToken,
            now,
            refreshSession.AbsoluteExpiresAtUtc,
            cancellationToken);

        refreshSession.LastUsedAtUtc = now;
        refreshSession.RevokedAtUtc = now;
        refreshSession.RevocationReason = "Rotated";
        refreshSession.ReplacedBySessionId = newSession.Id;
        await dbContext.SaveChangesAsync(cancellationToken);

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

        var now = DateTimeOffset.UtcNow;
        await RevokeAllActiveSessionsByUserAsync(user.Id, now, "Logout", cancellationToken);

        // Revokes existing refresh tokens tied to prior security stamp.
        await userManager.UpdateSecurityStampAsync(user);
        logger.LogInformation("AUDIT: Logout success for userId {UserId}", user.Id);
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
        var originalStatusCode = httpContext.Response.StatusCode;
        var originalContentType = httpContext.Response.ContentType;
        var originalContentLength = httpContext.Response.ContentLength;
        var originalHeaders = httpContext.Response.Headers
            .ToDictionary(pair => pair.Key, pair => pair.Value);

        httpContext.Response.Body = buffer;

        try
        {
            await httpContext.SignInAsync(IdentityConstants.BearerScheme, principal);
            buffer.Position = 0;

            return await JsonSerializer.DeserializeAsync<IdentityBearerTokenPayload>(
                buffer,
                TokenJsonOptions,
                cancellationToken);
        }
        finally
        {
            httpContext.Response.Body = originalBody;
            httpContext.Response.StatusCode = originalStatusCode;
            httpContext.Response.ContentType = originalContentType;
            httpContext.Response.ContentLength = originalContentLength;
            httpContext.Response.Headers.Clear();

            foreach (var (key, value) in originalHeaders)
            {
                httpContext.Response.Headers[key] = value;
            }
        }
    }

    private async Task<AppRefreshSession> CreateRefreshSessionAsync(
        int userId,
        string refreshToken,
        DateTimeOffset now,
        DateTimeOffset absoluteExpiresAtUtc,
        CancellationToken cancellationToken)
    {
        var (ip, userAgent) = GetClientContext();
        var refreshSession = new AppRefreshSession
        {
            UserId = userId,
            TokenHash = ComputeTokenHash(refreshToken),
            CreatedAtUtc = now,
            LastUsedAtUtc = now,
            AbsoluteExpiresAtUtc = absoluteExpiresAtUtc,
            CreatedByIp = ip,
            UserAgent = userAgent
        };

        dbContext.RefreshSessions.Add(refreshSession);
        await dbContext.SaveChangesAsync(cancellationToken);
        return refreshSession;
    }

    private async Task HandleRefreshReuseAsync(
        AppRefreshSession refreshSession,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        refreshSession.ReuseDetectedAtUtc = now;
        refreshSession.RevocationReason ??= "ReuseDetected";
        await RevokeAllActiveSessionsByUserAsync(refreshSession.UserId, now, "ReuseDetected", cancellationToken);

        logger.LogWarning("AUDIT: Refresh reuse detected for userId {UserId}, sessionId {SessionId}",
            refreshSession.UserId,
            refreshSession.Id);
    }

    private async Task RevokeAllActiveSessionsByUserAsync(
        int userId,
        DateTimeOffset now,
        string reason,
        CancellationToken cancellationToken)
    {
        var activeSessions = await dbContext.RefreshSessions
            .Where(x => x.UserId == userId && !x.RevokedAtUtc.HasValue && !x.ReplacedBySessionId.HasValue)
            .ToListAsync(cancellationToken);

        foreach (var session in activeSessions)
        {
            session.RevokedAtUtc = now;
            session.RevocationReason = reason;
            session.LastUsedAtUtc = now;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task RevokeSessionAsync(
        AppRefreshSession refreshSession,
        DateTimeOffset now,
        string reason,
        CancellationToken cancellationToken)
    {
        refreshSession.RevokedAtUtc = now;
        refreshSession.RevocationReason = reason;
        refreshSession.LastUsedAtUtc = now;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string ComputeTokenHash(string refreshToken)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToHexString(hash);
    }

    private (string? Ip, string? UserAgent) GetClientContext()
    {
        var context = httpContextAccessor.HttpContext;
        if (context is null)
        {
            return (null, null);
        }

        var ip = context.Connection.RemoteIpAddress?.ToString();
        var userAgent = context.Request.Headers.UserAgent.ToString();
        return (ip, string.IsNullOrWhiteSpace(userAgent) ? null : userAgent[..Math.Min(userAgent.Length, 256)]);
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

    private void LogStepDuration(string step, long elapsedMilliseconds)
    {
        if (elapsedMilliseconds >= performanceOptions.Value.SlowStepThresholdMs &&
            logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Identity session step {Step} took {ElapsedMs} ms", step, elapsedMilliseconds);
        }
    }

}
