namespace Scheduling.Application.Features.Tasks.Shared;

/// <summary>
/// A schedule as the admin edits it (platform/007 R-02): <c>kind</c> is <c>interval</c>, <c>daily</c> or <c>manual</c>;
/// <c>times</c> are Kyiv <c>HH:mm</c>; <c>days</c> are ISO weekdays, 1 = Monday … 7 = Sunday.
/// </summary>
public sealed record ScheduleInput(
    string Kind,
    int? EveryMinutes = null,
    int? FromHour = null,
    int? ToHour = null,
    IReadOnlyList<string>? Times = null,
    IReadOnlyList<int>? Days = null);
