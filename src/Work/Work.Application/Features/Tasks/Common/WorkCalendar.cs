namespace Work.Application.Features.Tasks.Common;

/// <summary>Work days follow the Kyiv calendar: a due date passes at Kyiv midnight.</summary>
internal static class WorkCalendar
{
    private static readonly TimeZoneInfo Kyiv = TimeZoneInfo.FindSystemTimeZoneById("Europe/Kyiv");

    public static DateOnly Today(TimeProvider clock) =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(clock.GetUtcNow(), Kyiv).DateTime);

    public static DateTimeOffset MidnightUtc(DateOnly date)
    {
        var local = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified);
        return new DateTimeOffset(local, Kyiv.GetUtcOffset(local)).ToUniversalTime();
    }
}
