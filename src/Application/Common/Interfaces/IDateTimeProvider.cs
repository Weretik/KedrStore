namespace Application.Common.Interfaces
{
    public interface IDateTimeProvider
    {
        DateTime UtcNow { get; }
        DateTime UtcToday { get; }
        DateTime GetCurrentTime(string timeZoneId);
        DateTime ConvertToLocalTime(DateTime utcTime, string timeZoneId);
        TimeSpan GetTimeZoneOffset(string timeZoneId);
    }
}
