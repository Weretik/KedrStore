using Scheduling.Application.Features.Catalog;

namespace Scheduling.Application.Features.Tasks.Shared;

public static class ScheduledTaskMapper
{
    private static readonly TimeSpan OverdueAfter = TimeSpan.FromMinutes(15);

    public static ScheduledTaskResponse ToResponse(ScheduledTaskDefinition definition, ScheduledTask task, DateTimeOffset now) => new(
        task.Key, definition.Name, definition.Description, definition.Editable, task.Enabled,
        ScheduleInputMapper.ToInput(task.Schedule), task.NextRunAt, task.State == TaskState.Running, task.RunningSince,
        task.RunRequestedAt, task.LastSuccessAt, task.LastSuccessSummary, task.LastFailureAt, task.LastFailureReason,
        LastRunFailed: task.LastFailureAt is { } failed && (task.LastSuccessAt is null || failed > task.LastSuccessAt),
        Overdue: task.Enabled && task.State == TaskState.Idle && task.NextRunAt is { } next && next + OverdueAfter < now);

    public static ScheduledTaskRunResponse ToResponse(ScheduledTaskRun run) => new(
        run.Id, run.Trigger == RunTrigger.Manual ? "manual" : "schedule", run.RequestedBy, run.ClaimedAt, run.StartedAt,
        run.FinishedAt, run.Outcome switch { RunOutcome.Succeeded => "succeeded", RunOutcome.Failed => "failed", _ => "running" },
        run.Message);
}
