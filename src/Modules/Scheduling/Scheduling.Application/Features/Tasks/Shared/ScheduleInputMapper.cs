using System.Globalization;

namespace Scheduling.Application.Features.Tasks.Shared;

/// <summary>Turns <see cref="ScheduleInput"/> into a domain schedule and back.</summary>
public static class ScheduleInputMapper
{
    public const string Interval = "interval";
    public const string Daily = "daily";
    public const string Manual = "manual";

    public static DomainResult<TaskSchedule> ToSchedule(ScheduleInput input) => input.Kind switch
    {
        Interval => TaskSchedule.Interval(input.EveryMinutes ?? 0, input.FromHour, input.ToHour),
        Daily => TaskSchedule.Daily((input.Times ?? []).Select(ParseTime), (input.Days ?? []).Select(ToDayOfWeek)),
        _ => TaskSchedule.Manual
    };

    public static ScheduleInput ToInput(TaskSchedule schedule) => schedule.Kind switch
    {
        ScheduleKind.Interval => new ScheduleInput(Interval, schedule.EveryMinutes, schedule.FromHour, schedule.ToHour),
        ScheduleKind.Daily => new ScheduleInput(Daily,
            Times: schedule.Times.Select(x => x.ToString("HH:mm", CultureInfo.InvariantCulture)).ToArray(),
            Days: schedule.Days.Select(ToIsoDay).ToArray()),
        _ => new ScheduleInput(Manual)
    };

    public static bool TryParseTime(string? value, out TimeOnly time) =>
        TimeOnly.TryParseExact(value, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out time);

    private static TimeOnly ParseTime(string value) => TryParseTime(value, out var time) ? time : TimeOnly.MinValue;

    private static DayOfWeek ToDayOfWeek(int isoDay) => (DayOfWeek)(isoDay % 7);

    private static int ToIsoDay(DayOfWeek day) => day == DayOfWeek.Sunday ? 7 : (int)day;
}
