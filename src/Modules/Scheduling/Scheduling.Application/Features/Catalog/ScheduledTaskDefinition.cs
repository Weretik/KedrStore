namespace Scheduling.Application.Features.Catalog;

/// <summary>
/// One task of the code catalogue (platform/007 R-01). <paramref name="MaxMinutes"/> is the longest allowed run;
/// <paramref name="RunsInTick"/> marks work done inside the tick instead of the job; <paramref name="Editable"/> lets
/// the admin change the schedule; <paramref name="EnabledByDefault"/> is the switch of a new row (R-12).
/// Tasks with the same <paramref name="Group"/> write the same tables and never run at once: one waits for the next
/// tick while another of its group runs (database rules 4).
/// </summary>
public sealed record ScheduledTaskDefinition(
    string Key,
    string Name,
    string Description,
    TaskSchedule DefaultSchedule,
    int MaxMinutes,
    bool Editable = true,
    bool RunsInTick = false,
    bool EnabledByDefault = true,
    string? Group = null);
