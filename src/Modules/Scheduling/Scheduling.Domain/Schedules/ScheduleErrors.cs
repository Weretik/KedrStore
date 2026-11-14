namespace Scheduling.Domain.Schedules;

/// <summary>Rules a schedule breaks; messages are shown to the admin as they are.</summary>
public static class ScheduleErrors
{
    public const int MinIntervalMinutes = 1;
    public const int MaxIntervalMinutes = 1440;
    public const int MaxTimes = 6;

    public static DomainError IntervalOutOfRange { get; } = new("Scheduling.Schedule.Interval", $"Інтервал має бути від {MinIntervalMinutes} до {MaxIntervalMinutes} хвилин.", Field: "schedule.everyMinutes");

    public static DomainError WindowInvalid { get; } = new("Scheduling.Schedule.Window", "Години роботи: «з» і «до» від 0 до 23, «з» менше за «до».", Field: "schedule.fromHour");

    public static DomainError TimesInvalid { get; } = new("Scheduling.Schedule.Times", $"Вкажіть від 1 до {MaxTimes} різних часів запуску.", Field: "schedule.times");

    public static DomainError DaysInvalid { get; } = new("Scheduling.Schedule.Days", "Оберіть хоча б один день тижня.", Field: "schedule.days");
}
