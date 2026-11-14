namespace Scheduling.Api.Endpoints.Tick.RunSchedulerTick;

/// <summary>
/// Who may call the tick (platform/007 R-10): Cloud Scheduler with an OIDC token of <see cref="SchedulerServiceAccount"/>
/// for <see cref="Audience"/>. Without both the endpoint answers 404.
/// </summary>
public sealed class SchedulerTickOptions
{
    public const string Section = "Scheduler";

    public string? Audience { get; set; }
    public string? SchedulerServiceAccount { get; set; }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Audience) && !string.IsNullOrWhiteSpace(SchedulerServiceAccount);
}
