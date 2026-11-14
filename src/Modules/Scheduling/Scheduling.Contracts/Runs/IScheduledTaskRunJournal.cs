namespace Scheduling.Contracts.Runs;

/// <summary>
/// What the tasks job (Host.Jobs <c>--tasks</c>) records about a run the tick claimed (spec platform/007 R-06). A run
/// that is no longer the task's current one is refused, and the job skips it.
/// </summary>
public interface IScheduledTaskRunJournal
{
    /// <summary>Marks the run started; false when it is stale or unknown.</summary>
    Task<bool> StartAsync(string key, Guid runId, CancellationToken cancellationToken);

    /// <summary>Records the summary of a success or the main reason of a failure and frees the task.</summary>
    Task FinishAsync(string key, Guid runId, bool succeeded, string message, CancellationToken cancellationToken);
}
