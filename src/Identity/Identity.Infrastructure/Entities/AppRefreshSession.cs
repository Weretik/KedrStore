namespace Identity.Infrastructure.Entities;

public class AppRefreshSession
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;

    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset LastUsedAtUtc { get; set; }
    public DateTimeOffset AbsoluteExpiresAtUtc { get; set; }

    public DateTimeOffset? RevokedAtUtc { get; set; }
    public string? RevocationReason { get; set; }
    public int? ReplacedBySessionId { get; set; }
    public DateTimeOffset? ReuseDetectedAtUtc { get; set; }

    public string? CreatedByIp { get; set; }
    public string? UserAgent { get; set; }

    public AppUser User { get; set; } = null!;
}
