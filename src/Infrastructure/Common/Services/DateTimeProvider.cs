using Application.Common.Interfaces;

namespace Infrastructure.Common.Services;

public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;

    public DateTime UtcToday => DateTime.UtcNow.Date;

    public DateTime GetCurrentTime(string timeZoneId)
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        return TimeZoneInfo.ConvertTimeFromUtc(UtcNow, tz);
    }

    public DateTime ConvertToLocalTime(DateTime utcTime, string timeZoneId)
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        return TimeZoneInfo.ConvertTimeFromUtc(utcTime, tz);
    }

    public TimeSpan GetTimeZoneOffset(string timeZoneId)
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
        return tz.GetUtcOffset(UtcNow);
    }
}
