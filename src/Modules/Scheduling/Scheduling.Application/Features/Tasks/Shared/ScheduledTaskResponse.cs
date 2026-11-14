namespace Scheduling.Application.Features.Tasks.Shared;

/// <summary>
/// One row of «Фонові задачі» (platform/007 R-09). <see cref="LastRunFailed"/> and <see cref="Overdue"/> are the only
/// marks: the last result was a failure, or an enabled task has missed its time by more than 15 minutes.
/// </summary>
public sealed record ScheduledTaskResponse(
    string Key,
    string Name,
    string Description,
    bool Editable,
    bool Enabled,
    ScheduleInput Schedule,
    DateTimeOffset? NextRunAt,
    bool IsRunning,
    DateTimeOffset? RunningSince,
    DateTimeOffset? RunRequestedAt,
    DateTimeOffset? LastSuccessAt,
    string? LastSuccessSummary,
    DateTimeOffset? LastFailureAt,
    string? LastFailureReason,
    bool LastRunFailed,
    bool Overdue);

/// <summary>One run in the history of a task.</summary>
public sealed record ScheduledTaskRunResponse(
    Guid Id,
    string Trigger,
    string? RequestedBy,
    DateTimeOffset ClaimedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? FinishedAt,
    string Outcome,
    string? Message);
