namespace Scheduling.Infrastructure.Integrations.CloudRun;

/// <summary>The Cloud Run Job that runs the tasks (platform/007 EN-02): <c>Scheduler:Job:{Project,Region,Name}</c>.</summary>
public sealed class CloudRunJobOptions
{
    public const string Section = "Scheduler:Job";

    public string? Project { get; set; }
    public string? Region { get; set; }
    public string? Name { get; set; }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Project) && !string.IsNullOrWhiteSpace(Region) && !string.IsNullOrWhiteSpace(Name);
}
