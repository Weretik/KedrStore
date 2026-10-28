using System.Security.Cryptography;
using Identity.Infrastructure.DataBase;
using Identity.Infrastructure.Entities;

namespace Identity.Infrastructure.Services.Sessions;

/// <summary>
/// Stores refresh sessions by token hash: create, find, rotate and revoke.
/// </summary>
public sealed class RefreshSessionStore(
    AppIdentityDbContext dbContext,
    IHttpContextAccessor httpContextAccessor)
{
    public const string RotatedReason = "Rotated";

    public Task<AppRefreshSession?> FindByTokenAsync(string refreshToken, CancellationToken cancellationToken) =>
        dbContext.RefreshSessions.SingleOrDefaultAsync(x => x.TokenHash == ComputeTokenHash(refreshToken), cancellationToken);

    public async Task<AppRefreshSession> CreateAsync(
        Guid userId,
        string refreshToken,
        DateTimeOffset now,
        DateTimeOffset absoluteExpiresAtUtc,
        CancellationToken cancellationToken)
    {
        var refreshSession = NewSession(userId, refreshToken, now, absoluteExpiresAtUtc);
        dbContext.RefreshSessions.Add(refreshSession);
        await dbContext.SaveChangesAsync(cancellationToken);
        return refreshSession;
    }

    /// <summary>
    /// Replaces the session with a new one in one save. Returns null when another request rotated it first.
    /// </summary>
    public async Task<AppRefreshSession?> RotateAsync(
        AppRefreshSession refreshSession,
        string newRefreshToken,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        refreshSession.LastUsedAtUtc = now;
        refreshSession.RevokedAtUtc = now;
        refreshSession.RevocationReason = RotatedReason;

        try
        {
            // The xmin check fails here when a parallel request has already rotated this session.
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            dbContext.ChangeTracker.Clear();
            return null;
        }

        var newSession = NewSession(refreshSession.UserId, newRefreshToken, now, refreshSession.AbsoluteExpiresAtUtc);
        dbContext.RefreshSessions.Add(newSession);
        await dbContext.SaveChangesAsync(cancellationToken);

        refreshSession.ReplacedBySessionId = newSession.Id;
        await dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return newSession;
    }

    public async Task RevokeAsync(
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

    public async Task RevokeAllActiveByUserAsync(
        Guid userId,
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

    private AppRefreshSession NewSession(
        Guid userId,
        string refreshToken,
        DateTimeOffset now,
        DateTimeOffset absoluteExpiresAtUtc)
    {
        var (ip, userAgent) = GetClientContext();
        return new AppRefreshSession
        {
            UserId = userId,
            TokenHash = ComputeTokenHash(refreshToken),
            CreatedAtUtc = now,
            LastUsedAtUtc = now,
            AbsoluteExpiresAtUtc = absoluteExpiresAtUtc,
            CreatedByIp = ip,
            UserAgent = userAgent
        };
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
}
