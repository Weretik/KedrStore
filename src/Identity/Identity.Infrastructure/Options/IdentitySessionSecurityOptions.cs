namespace Identity.Infrastructure.Options;

public sealed class IdentitySessionSecurityOptions
{
    public const string SectionName = "Identity:SessionSecurity";

    public int AccessTokenLifetimeMinutes { get; init; } = 15;
    public int RefreshAbsoluteLifetimeDays { get; init; } = 30;
    public int RefreshIdleTimeoutDays { get; init; } = 14;
}
