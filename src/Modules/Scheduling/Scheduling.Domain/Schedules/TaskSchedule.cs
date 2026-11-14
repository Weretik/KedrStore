namespace Scheduling.Domain.Schedules;

/// <summary>
/// When a task runs (platform/007 R-02). Interval runs fall on a grid that starts at the window's first hour (or midnight)
/// of each Kyiv day, so «every 60 minutes from 8 to 20» is 08:00, 09:00 … 20:00. Daily runs are Kyiv wall-clock
/// times on the chosen weekdays.
/// </summary>
public sealed record TaskSchedule
{
    private TaskSchedule(ScheduleKind kind, int? everyMinutes, int? fromHour, int? toHour,
        IReadOnlyList<TimeOnly> times, IReadOnlyList<DayOfWeek> days)
    {
        Kind = kind;
        EveryMinutes = everyMinutes;
        FromHour = fromHour;
        ToHour = toHour;
        Times = times;
        Days = days;
    }

    public ScheduleKind Kind { get; }
    public int? EveryMinutes { get; }
    public int? FromHour { get; }
    public int? ToHour { get; }
    public IReadOnlyList<TimeOnly> Times { get; }
    public IReadOnlyList<DayOfWeek> Days { get; }

    public static TaskSchedule Manual { get; } = new(ScheduleKind.Manual, null, null, null, [], []);

    public static IReadOnlyList<DayOfWeek> Weekdays { get; } =
        [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday];

    public static IReadOnlyList<DayOfWeek> EveryDay { get; } =
        [.. Weekdays, DayOfWeek.Saturday, DayOfWeek.Sunday];

    public static DomainResult<TaskSchedule> Interval(int everyMinutes, int? fromHour = null, int? toHour = null)
    {
        if (everyMinutes is < ScheduleErrors.MinIntervalMinutes or > ScheduleErrors.MaxIntervalMinutes)
            return ScheduleErrors.IntervalOutOfRange;
        if (fromHour.HasValue != toHour.HasValue || fromHour is < 0 or > 23 || toHour is < 0 or > 23 || fromHour >= toHour)
            return ScheduleErrors.WindowInvalid;
        return new TaskSchedule(ScheduleKind.Interval, everyMinutes, fromHour, toHour, [], []);
    }

    public static DomainResult<TaskSchedule> Daily(IEnumerable<TimeOnly> times, IEnumerable<DayOfWeek> days)
    {
        var distinctTimes = times.Select(x => new TimeOnly(x.Hour, x.Minute)).Distinct().Order().ToArray();
        if (distinctTimes.Length is 0 or > ScheduleErrors.MaxTimes) return ScheduleErrors.TimesInvalid;
        var distinctDays = days.Distinct().Where(Enum.IsDefined).OrderBy(x => ((int)x + 6) % 7).ToArray();
        if (distinctDays.Length == 0) return ScheduleErrors.DaysInvalid;
        return new TaskSchedule(ScheduleKind.Daily, null, null, null, distinctTimes, distinctDays);
    }

    /// <summary>A schedule written in code (the catalogue); a mistake there is a programming error.</summary>
    public static TaskSchedule DailyAt(IReadOnlyList<DayOfWeek> days, params TimeOnly[] times) => Daily(times, days).Value;

    /// <summary>The first run strictly after <paramref name="instant"/>; null for a manual task.</summary>
    public DateTimeOffset? NextAfter(DateTimeOffset instant) => Kind switch
    {
        ScheduleKind.Interval => NextInterval(instant),
        ScheduleKind.Daily => NextDaily(instant),
        _ => null
    };

    private DateTimeOffset NextInterval(DateTimeOffset instant)
    {
        var day = KyivTime.ToLocal(instant).Date;
        var step = TimeSpan.FromMinutes(EveryMinutes!.Value);
        var first = TimeSpan.FromHours(FromHour ?? 0);
        var last = ToHour is { } to ? TimeSpan.FromHours(to) : TimeSpan.FromDays(1) - TimeSpan.FromTicks(1);
        for (var offset = 0; offset < 3; offset++, day = day.AddDays(1))
        {
            for (var at = first; at <= last; at += step)
            {
                var candidate = KyivTime.ToInstant(day + at);
                if (candidate > instant) return candidate;
            }
        }

        throw new UnreachableException("An interval schedule always has a run within three days.");
    }

    private DateTimeOffset NextDaily(DateTimeOffset instant)
    {
        var day = KyivTime.ToLocal(instant).Date;
        for (var offset = 0; offset < 8; offset++, day = day.AddDays(1))
        {
            if (!Days.Contains(day.DayOfWeek)) continue;
            foreach (var time in Times)
            {
                var candidate = KyivTime.ToInstant(day + time.ToTimeSpan());
                if (candidate > instant) return candidate;
            }
        }

        throw new UnreachableException("A daily schedule always has a run within eight days.");
    }

    public bool Equals(TaskSchedule? other) =>
        other is not null && Kind == other.Kind && EveryMinutes == other.EveryMinutes && FromHour == other.FromHour &&
        ToHour == other.ToHour && Times.SequenceEqual(other.Times) && Days.SequenceEqual(other.Days);

    public override int GetHashCode() => HashCode.Combine(Kind, EveryMinutes, FromHour, ToHour, Times.Count, Days.Count);
}
