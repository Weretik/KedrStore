namespace Identity.Infrastructure.Options;

public sealed class IdentitySessionSecurityOptions
{
    public const string SectionName = "Identity:SessionSecurity";

    public int AccessTokenLifetimeMinutes { get; init; } = 15;
    public int RefreshAbsoluteLifetimeDays { get; init; } = 30;
    public int RefreshIdleTimeoutDays { get; init; } = 14;
    // A token rotated less than this many seconds ago is rejected quietly instead of treated as stolen.
    public int RefreshReuseGraceSeconds { get; init; } = 60;
}
