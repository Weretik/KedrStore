namespace Scheduling.Domain.Schedules;

/// <summary>How a task repeats (platform/007 R-02).</summary>
public enum ScheduleKind
{
    /// <summary>Every N minutes, optionally only between two Kyiv hours.</summary>
    Interval = 0,

    /// <summary>At one to six Kyiv times on the chosen weekdays.</summary>
    Daily = 1,

    /// <summary>Only when someone presses «Запустити зараз».</summary>
    Manual = 2
}
