using Scheduling.Application.Features.Tasks.Shared;

namespace Scheduling.Application.Features.Tick.Commands.RunSchedulerTick;

/// <summary>
/// platform/007 R-04: (a) inline work (outbox), (b) stuck runs failed, (c, d) due tasks claimed one by one with the
/// concurrency check, (e) one job execution for all of them, R-05 when it cannot start, (f) old runs deleted.
/// </summary>
public sealed class RunSchedulerTickHandler(
    ISchedulingDbContext db,
    TimeProvider clock,
    IEnumerable<ITickWork> inlineWork,
    ITaskJobLauncher launcher,
    ILogger<RunSchedulerTickHandler> logger) : ICommandHandler<RunSchedulerTickCommand, Result<SchedulerTickResponse>>
{
    public static readonly TimeSpan KeepRuns = TimeSpan.FromDays(90);

    public async ValueTask<Result<SchedulerTickResponse>> Handle(RunSchedulerTickCommand command, CancellationToken cancellationToken)
    {
        var rows = await ScheduledTaskRows.LoadAsync(db, clock.GetUtcNow(), cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        var inline = await RunInlineAsync(rows, cancellationToken);
        await ExpireStuckAsync(rows, cancellationToken);
        var claimed = await ClaimDueAsync(rows, cancellationToken);
        var launchError = await LaunchAsync(claimed, cancellationToken);
        await DeleteOldRunsAsync(cancellationToken);

        return Result.Success(new SchedulerTickResponse(inline,
            launchError is null ? claimed.Select(x => x.Run.TaskKey).ToArray() : [], launchError));
    }

    private async Task<IReadOnlyDictionary<string, string>> RunInlineAsync(IReadOnlyList<ScheduledTaskRow> rows,
        CancellationToken cancellationToken)
    {
        var results = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var work in inlineWork)
        {
            var row = rows.FirstOrDefault(x => x.Definition.Key == work.TaskKey && x.Definition.RunsInTick);
            if (row is null || !row.Task.Enabled) continue;
            try
            {
                var summary = await work.RunAsync(cancellationToken);
                row.Task.RecordInline(true, summary, clock.GetUtcNow());
                results[work.TaskKey] = summary;
            }
            catch (Exception error) when (error is not OperationCanceledException)
            {
                logger.LogError(error, "Scheduler: {Task} failed in the tick", work.TaskKey);
                row.Task.RecordInline(false, error.Message, clock.GetUtcNow());
                results[work.TaskKey] = error.Message;
            }
        }

        await SaveQuietlyAsync(cancellationToken);
        return results;
    }

    private async Task ExpireStuckAsync(IReadOnlyList<ScheduledTaskRow> rows, CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        foreach (var row in rows.Where(x => x.Task.State == TaskState.Running))
        {
            var run = row.Task.CurrentRunId is { } id
                ? await db.Runs.FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
                : null;
            if (row.Task.ExpireIfStuck(run, row.Definition.MaxMinutes, now))
                logger.LogWarning("Scheduler: {Task} did not finish in {Minutes} minutes", row.Task.Key, row.Definition.MaxMinutes);
        }

        await SaveQuietlyAsync(cancellationToken);
    }

    private async Task<IReadOnlyList<(ScheduledTask Task, ScheduledTaskRun Run)>> ClaimDueAsync(
        IReadOnlyList<ScheduledTaskRow> rows, CancellationToken cancellationToken)
    {
        var claimed = new List<(ScheduledTask, ScheduledTaskRun)>();
        foreach (var row in rows.Where(x => !x.Definition.RunsInTick))
        {
            var now = clock.GetUtcNow();
            if (!row.Task.IsDue(now)) continue;
            var run = row.Task.Claim(now);
            db.Runs.Add(run);
            if (await SaveQuietlyAsync(cancellationToken)) claimed.Add((row.Task, run));
        }

        return claimed;
    }

    private async Task<string?> LaunchAsync(IReadOnlyList<(ScheduledTask Task, ScheduledTaskRun Run)> claimed,
        CancellationToken cancellationToken)
    {
        if (claimed.Count == 0) return null;
        string? error;
        try
        {
            error = await launcher.LaunchAsync(claimed.Select(x => new TaskLaunch(x.Task.Key, x.Run.Id)).ToArray(), cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogError(exception, "Scheduler: the tasks job did not start");
            error = exception.Message;
        }

        if (error is null)
        {
            logger.LogInformation("Scheduler: started {Tasks}", string.Join(", ", claimed.Select(x => x.Task.Key)));
            return null;
        }

        var reason = $"Не вдалося запустити задачу: {error}";
        foreach (var (task, run) in claimed) task.Fail(run, reason, clock.GetUtcNow());
        await SaveQuietlyAsync(cancellationToken);
        return error;
    }

    private async Task DeleteOldRunsAsync(CancellationToken cancellationToken)
    {
        var before = clock.GetUtcNow() - KeepRuns;
        await db.Runs.Where(x => x.ClaimedAt < before).ExecuteDeleteAsync(cancellationToken);
    }

    /// <summary>
    /// Saves pending changes; a concurrency conflict (another tick or the job changed the row) drops them and
    /// answers false, so that task is left to whoever changed it.
    /// </summary>
    private async Task<bool> SaveQuietlyAsync(CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateConcurrencyException conflict)
        {
            foreach (var entry in conflict.Entries) entry.State = EntityState.Detached;
            logger.LogInformation("Scheduler: a task changed meanwhile; left to the other change");
            return false;
        }
    }
}
