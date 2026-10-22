using System.Globalization;
using Sales.Application.Contracts.Orders;

namespace Sales.Api.Controllers;

internal readonly record struct WorkspaceDateRange(
    DateTimeOffset? FromUtc,
    DateTimeOffset? ToUtcExclusive)
{
    private static readonly TimeZoneInfo Kyiv = TimeZoneInfo.FindSystemTimeZoneById("Europe/Kyiv");

    internal static WorkspaceDateRange Parse(string? from, string? to)
    {
        var start = ParseDate(from);
        var end = ParseDate(to);
        if (start is { } first && end is { } last && first > last)
            throw InvalidRange();
        if (end == DateOnly.MaxValue)
            throw InvalidRange();

        return new WorkspaceDateRange(
            start is { } fromDate ? AtKyivMidnightUtc(fromDate) : null,
            end is { } toDate ? AtKyivMidnightUtc(toDate.AddDays(1)) : null);
    }

    private static DateOnly? ParseDate(string? value)
    {
        if (value is null) return null;
        if (value.Length != 10 ||
            !DateOnly.TryParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var parsed))
            throw InvalidRange();
        return parsed;
    }

    private static DateTimeOffset AtKyivMidnightUtc(DateOnly date)
    {
        var local = DateTime.SpecifyKind(date.ToDateTime(TimeOnly.MinValue), DateTimeKind.Unspecified);
        return new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(local, Kyiv), TimeSpan.Zero);
    }

    private static WorkspaceException InvalidRange()
        => new(400, "invalid_date_range", "Use valid YYYY-MM-DD dates with createdFrom not after createdTo.");
}
