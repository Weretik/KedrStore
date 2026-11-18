namespace Scheduling.Application.Features.Runs;

/// <summary>Loads a task with one of its runs, as the job addresses them.</summary>
internal static class ClaimedRuns
{
    public static async Task<(ScheduledTask Task, ScheduledTaskRun Run)?> FindAsync(ISchedulingDbContext db, string key,
        Guid runId, CancellationToken cancellationToken)
    {
        var taskKey = ScheduledTaskKey.FromStorage(key);
        var id = ScheduledTaskRunId.FromStorage(runId);
        var task = await db.Tasks.FirstOrDefaultAsync(x => x.Key == taskKey, cancellationToken);
        var run = await db.Runs.FirstOrDefaultAsync(x => x.Id == id && x.TaskKey == taskKey, cancellationToken);
        return task is null || run is null ? null : (task, run);
    }
}
