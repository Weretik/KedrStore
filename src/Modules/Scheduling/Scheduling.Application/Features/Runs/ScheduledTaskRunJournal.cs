using Scheduling.Contracts.Runs;

namespace Scheduling.Application.Features.Runs;

/// <summary>Start and finish of a claimed run (R-06, R-07), called by the tasks job between its tasks.</summary>
public sealed class ScheduledTaskRunJournal(ISchedulingDbContext db, TimeProvider clock, ILogger<ScheduledTaskRunJournal> logger)
    : IScheduledTaskRunJournal
{
    public async Task<bool> StartAsync(string key, Guid runId, CancellationToken cancellationToken)
    {
        var found = await ClaimedRuns.FindAsync(db, key, runId, cancellationToken);
        if (found is null || found.Value.Task.Start(found.Value.Run, clock.GetUtcNow()).IsFailure)
        {
            logger.LogWarning("Scheduler: run {RunId} of {Task} is not current; skipped", runId, key);
            return false;
        }

        await db.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task FinishAsync(string key, Guid runId, bool succeeded, string message,
        CancellationToken cancellationToken)
    {
        var found = await ClaimedRuns.FindAsync(db, key, runId, cancellationToken);
        if (found is null) return;
        var (task, run) = found.Value;
        var now = clock.GetUtcNow();
        var finished = succeeded ? task.Succeed(run, message, now) : task.Fail(run, message, now);
        if (finished.IsFailure)
        {
            logger.LogWarning("Scheduler: run {RunId} of {Task} finished after it was freed", runId, key);
            return;
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
