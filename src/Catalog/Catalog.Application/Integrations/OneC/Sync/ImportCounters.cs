namespace Catalog.Application.Integrations.OneC.Sync;

/// <summary>Per-root counters and duration of one import phase (R-024).</summary>
public sealed class ImportCounters
{
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

    public int Received { get; set; }
    public int Created { get; set; }
    public int Updated { get; set; }
    public int Unchanged { get; set; }
    public int Deleted { get; set; }
    public int Restored { get; set; }
    public int Skipped { get; set; }
    public int GuardBlocked { get; set; }

    public TimeSpan Elapsed => _stopwatch.Elapsed;

    public void Log(ILogger logger, string phase, string root)
        => logger.LogInformation(
            "{Phase} for {Root} finished in {ElapsedMs} ms. Received: {Received}, Created: {Created}, Updated: {Updated}, " +
            "Unchanged: {Unchanged}, Deleted: {Deleted}, Restored: {Restored}, Skipped: {Skipped}, GuardBlocked: {GuardBlocked}.",
            phase, root, (long)Elapsed.TotalMilliseconds, Received, Created, Updated, Unchanged, Deleted, Restored, Skipped,
            GuardBlocked);
}
