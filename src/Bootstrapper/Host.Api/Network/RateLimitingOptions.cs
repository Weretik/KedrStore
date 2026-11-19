namespace Host.Api.Network;

/// <summary>
/// Request limits per calling address and minute, from the <c>RateLimiting</c> configuration section. Raising or
/// lowering one needs no deploy of code: set <c>RateLimiting__GlobalPerMinute</c> (and so on) on the service. The
/// defaults are soft: they stop a flood or a runaway script, not a normal buyer or a busy manager.
/// </summary>
public sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    /// <summary>Every request except health checks and internal calls; the soft limit for all endpoints.</summary>
    public int GlobalPerMinute { get; init; } = 1200;

    /// <summary>Anonymous catalogue and content reads (policy <c>PublicRead</c>). The public site reads from its server, so one address carries all visitors.</summary>
    public int PublicReadPerMinute { get; init; } = 600;

    /// <summary>Product lists with search and filters (policy <c>PublicSearch</c>), the most expensive anonymous read.</summary>
    public int PublicSearchPerMinute { get; init; } = 300;

    /// <summary>File uploads of signed-in staff (policy <c>Upload</c>).</summary>
    public int UploadPerMinute { get; init; } = 60;

    /// <summary>Imports, synchronisations, reports and exports (policy <c>HeavyOperation</c>).</summary>
    public int HeavyOperationPerMinute { get; init; } = 20;
}
