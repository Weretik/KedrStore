using System.Globalization;

namespace Sales.Application.Features.OrderWorkspace.Shared;

/// <summary>
/// A list filter by creation date: inclusive Kyiv calendar days as UTC instants, the end being the next midnight.
/// </summary>
public readonly record struct WorkspaceDateRange(DateTimeOffset? FromUtc, DateTimeOffset? ToUtcExclusive)
{
    public const string InvalidMessage = "Вкажіть коректні дати; початок не може бути пізніше за кінець.";

    private static readonly TimeZoneInfo Kyiv = TimeZoneInfo.FindSystemTimeZoneById("Europe/Kyiv");

    public static DomainResult<WorkspaceDateRange> Parse(string? from, string? to)
    {
        if (!TryParseDate(from, out var start) || !TryParseDate(to, out var end) ||
            start is { } first && end is { } last && first > last || end == DateOnly.MaxValue)
            return new DomainError("invalid_date_range", InvalidMessage);

        return new WorkspaceDateRange(
            start is { } fromDate ? AtKyivMidnightUtc(fromDate) : null,
            end is { } toDate ? AtKyivMidnightUtc(toDate.AddDays(1)) : null);
    }

    /// <summary>Start of the current Kyiv day, as the date filters count days.</summary>
    public static DateTimeOffset TodayStartUtc(DateTimeOffset now)
        => AtKyivMidnightUtc(DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(now, Kyiv).DateTime));

    private static bool TryParseDate(string? value, out DateOnly? date)
    {
        date = null;
        if (value is null) return true;
        if (value.Length != 10 || !DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var parsed))
            return false;
        date = parsed;
        return true;
    }

    private static DateTimeOffset AtKyivMidnightUtc(DateOnly date)
    {
        var local = DateTime.SpecifyKind(date.ToDateTime(TimeOnly.MinValue), DateTimeKind.Unspecified);
        return new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(local, Kyiv), TimeSpan.Zero);
    }
}
