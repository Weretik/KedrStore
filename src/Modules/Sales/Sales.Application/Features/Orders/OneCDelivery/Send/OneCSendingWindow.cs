using Microsoft.Extensions.Options;

namespace Sales.Application.Features.Orders.OneCDelivery.Send;

/// <summary>Orders go to 1C only between the window start and end, Kyiv time (platform spec 006, R-011).</summary>
public sealed class OneCSendingWindow(IOptions<OneCSendingWindowOptions> options)
{
    private static readonly TimeZoneInfo Kyiv = FindKyiv();

    private readonly OneCSendingWindowOptions _options = options.Value;

    /// <summary>Null inside the window; otherwise the next window start, in UTC.</summary>
    public DateTimeOffset? NextOpeningAfter(DateTimeOffset nowUtc)
    {
        var local = TimeZoneInfo.ConvertTime(nowUtc, Kyiv);
        var time = local.TimeOfDay;
        if (time >= _options.SendingWindowStart && time < _options.SendingWindowEnd)
            return null;

        var day = time < _options.SendingWindowStart ? local.Date : local.Date.AddDays(1);
        var opening = DateTime.SpecifyKind(day + _options.SendingWindowStart, DateTimeKind.Unspecified);
        return new DateTimeOffset(TimeZoneInfo.ConvertTimeToUtc(opening, Kyiv), TimeSpan.Zero);
    }

    private static TimeZoneInfo FindKyiv()
    {
        foreach (var id in (string[])["Europe/Kyiv", "Europe/Kiev", "FLE Standard Time"])
        {
            if (TimeZoneInfo.TryFindSystemTimeZoneById(id, out var zone))
                return zone;
        }

        throw new InvalidOperationException("The Kyiv time zone is not installed.");
    }
}
