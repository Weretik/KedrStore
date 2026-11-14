using System.Globalization;
using Scheduling.Domain.Schedules;

namespace UnitTests.Scheduling.Domain;

public sealed class TaskScheduleTests
{
    private static DateTimeOffset Kyiv(string local) => KyivTime.ToInstant(DateTime.Parse(local, CultureInfo.InvariantCulture));

    [Theory]
    [InlineData("2026-11-16 06:59", "2026-11-16 07:00")]
    [InlineData("2026-11-16 07:00", "2026-11-16 13:00")]
    [InlineData("2026-11-16 13:00", "2026-11-17 07:00")]
    [InlineData("2026-11-16 23:30", "2026-11-17 07:00")]
    public void Daily_twice_a_day_takes_the_next_Kyiv_time(string now, string expected)
    {
        var schedule = TaskSchedule.Daily([new TimeOnly(13, 0), new TimeOnly(7, 0)], TaskSchedule.EveryDay).Value;
        Assert.Equal(Kyiv(expected), schedule.NextAfter(Kyiv(now)));
    }

    [Theory]
    [InlineData("2026-11-13 09:00", "2026-11-16 09:00")] // Friday after the run → Monday
    [InlineData("2026-11-14 12:00", "2026-11-16 09:00")] // Saturday
    public void Weekdays_skip_the_weekend(string now, string expected)
    {
        var schedule = TaskSchedule.DailyAt(TaskSchedule.Weekdays, new TimeOnly(9, 0));
        Assert.Equal(Kyiv(expected), schedule.NextAfter(Kyiv(now)));
    }

    [Fact]
    public void Weekly_runs_on_Monday_only()
    {
        var schedule = TaskSchedule.DailyAt([DayOfWeek.Monday], new TimeOnly(6, 0));
        Assert.Equal(Kyiv("2026-11-23 06:00"), schedule.NextAfter(Kyiv("2026-11-16 06:00")));
    }

    [Theory]
    [InlineData("2026-11-16 07:10", "2026-11-16 08:00")]
    [InlineData("2026-11-16 08:00", "2026-11-16 09:00")]
    [InlineData("2026-11-16 20:00", "2026-11-17 08:00")]
    public void Interval_inside_a_window_runs_on_the_hour_grid(string now, string expected)
    {
        var schedule = TaskSchedule.Interval(60, 8, 20).Value;
        Assert.Equal(Kyiv(expected), schedule.NextAfter(Kyiv(now)));
    }

    [Fact]
    public void Interval_without_a_window_starts_at_midnight()
    {
        var schedule = TaskSchedule.Interval(10).Value;
        Assert.Equal(Kyiv("2026-11-16 10:10"), schedule.NextAfter(Kyiv("2026-11-16 10:03")));
        Assert.Equal(Kyiv("2026-11-17 00:00"), schedule.NextAfter(Kyiv("2026-11-16 23:55")));
    }

    [Fact]
    public void Daily_time_keeps_the_Kyiv_clock_across_the_autumn_change()
    {
        var schedule = TaskSchedule.DailyAt(TaskSchedule.EveryDay, new TimeOnly(3, 0));
        var beforeChange = schedule.NextAfter(Kyiv("2026-10-24 01:00"))!.Value;
        var changeDay = schedule.NextAfter(beforeChange)!.Value;
        var afterChange = schedule.NextAfter(changeDay)!.Value;
        Assert.Equal(TimeSpan.FromHours(3), beforeChange.Offset);
        Assert.Equal(new DateTime(2026, 10, 25, 3, 0, 0), KyivTime.ToLocal(changeDay));
        Assert.Equal(TimeSpan.FromHours(2), afterChange.Offset);
        Assert.Equal(new DateTime(2026, 10, 26, 3, 0, 0), KyivTime.ToLocal(afterChange));
    }

    [Fact]
    public void Manual_has_no_next_run() =>
        Assert.Null(TaskSchedule.Manual.NextAfter(DateTimeOffset.UtcNow));

    [Theory]
    [InlineData(0, null, null)]
    [InlineData(1441, null, null)]
    [InlineData(60, 8, null)]
    [InlineData(60, 20, 8)]
    [InlineData(60, 8, 24)]
    public void Invalid_interval_is_refused(int minutes, int? from, int? to) =>
        Assert.True(TaskSchedule.Interval(minutes, from, to).IsFailure);

    [Fact]
    public void Daily_needs_times_and_days()
    {
        Assert.Equal(ScheduleErrors.TimesInvalid, TaskSchedule.Daily([], TaskSchedule.EveryDay).Error);
        Assert.Equal(ScheduleErrors.DaysInvalid, TaskSchedule.Daily([new TimeOnly(9, 0)], []).Error);
        var seven = Enumerable.Range(0, 7).Select(h => new TimeOnly(h, 0));
        Assert.Equal(ScheduleErrors.TimesInvalid, TaskSchedule.Daily(seven, TaskSchedule.EveryDay).Error);
    }

    [Fact]
    public void Equal_schedules_compare_by_value() =>
        Assert.Equal(TaskSchedule.Daily([new TimeOnly(9, 0)], TaskSchedule.Weekdays).Value, TaskSchedule.DailyAt(TaskSchedule.Weekdays, new TimeOnly(9, 0)));
}
