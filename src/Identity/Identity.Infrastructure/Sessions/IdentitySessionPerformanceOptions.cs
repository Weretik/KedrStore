namespace Identity.Infrastructure.Sessions;

public sealed class IdentitySessionPerformanceOptions
{
    public const string SectionName = "Identity:SessionPerformance";

    public int SlowStepThresholdMs { get; init; } = 200;
}
