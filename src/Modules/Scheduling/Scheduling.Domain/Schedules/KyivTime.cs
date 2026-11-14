namespace Scheduling.Domain.Schedules;

/// <summary>Schedules follow the clock in Kyiv, whatever the server time zone is.</summary>
public static class KyivTime
{
    public static TimeZoneInfo Zone { get; } = TimeZoneInfo.FindSystemTimeZoneById("Europe/Kyiv");

    /// <summary>The instant of a Kyiv wall-clock time; a time skipped by the spring change moves one hour later.</summary>
    public static DateTimeOffset ToInstant(DateTime local)
    {
        var unspecified = DateTime.SpecifyKind(local, DateTimeKind.Unspecified);
        while (Zone.IsInvalidTime(unspecified)) unspecified = unspecified.AddHours(1);
        return new DateTimeOffset(unspecified, Zone.GetUtcOffset(unspecified));
    }

    public static DateTime ToLocal(DateTimeOffset instant) => TimeZoneInfo.ConvertTime(instant, Zone).DateTime;
}
