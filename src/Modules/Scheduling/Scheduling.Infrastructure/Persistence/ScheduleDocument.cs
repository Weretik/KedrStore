using System.Text.Json;
using Scheduling.Domain.Schedules;

namespace Scheduling.Infrastructure.Persistence;

/// <summary>The jsonb form of a <see cref="TaskSchedule"/> in <c>scheduling."ScheduledTasks"."Schedule"</c>.</summary>
internal sealed record ScheduleDocument(ScheduleKind Kind, int? EveryMinutes, int? FromHour, int? ToHour,
    TimeOnly[] Times, DayOfWeek[] Days)
{
    public static string Write(TaskSchedule schedule) => JsonSerializer.Serialize(new ScheduleDocument(schedule.Kind,
        schedule.EveryMinutes, schedule.FromHour, schedule.ToHour, [.. schedule.Times], [.. schedule.Days]));

    /// <summary>A stored schedule was valid when saved; one that no longer is becomes manual instead of failing reads.</summary>
    public static TaskSchedule Read(string json)
    {
        var document = JsonSerializer.Deserialize<ScheduleDocument>(json);
        var schedule = document?.Kind switch
        {
            ScheduleKind.Interval => TaskSchedule.Interval(document.EveryMinutes ?? 0, document.FromHour, document.ToHour),
            ScheduleKind.Daily => TaskSchedule.Daily(document.Times, document.Days),
            _ => null
        };
        return schedule is { IsSuccess: true } ? schedule.Value : TaskSchedule.Manual;
    }
}
