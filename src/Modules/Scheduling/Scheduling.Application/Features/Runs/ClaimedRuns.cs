namespace Scheduling.Application.Features.Runs;

/// <summary>Loads a task with one of its runs, as the job addresses them.</summary>
internal static class ClaimedRuns
{
    public static async Task<(ScheduledTask Task, ScheduledTaskRun Run)?> FindAsync(ISchedulingDbContext db, string key,
        Guid runId, CancellationToken cancellationToken)
    {
        var task = await db.Tasks.FirstOrDefaultAsync(x => x.Key == key, cancellationToken);
        var run = await db.Runs.FirstOrDefaultAsync(x => x.Id == runId && x.TaskKey == key, cancellationToken);
        return task is null || run is null ? null : (task, run);
    }
}
