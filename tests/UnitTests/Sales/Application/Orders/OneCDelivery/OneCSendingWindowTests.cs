using Microsoft.Extensions.Options;
using Sales.Application.Features.Orders.OneCDelivery.Send;

namespace UnitTests.Sales.Application.Orders.OneCDelivery;

/// <summary>Platform spec 006, R-011, SC-012: orders go to 1C only 07:00–20:00 Kyiv time.</summary>
public sealed class OneCSendingWindowTests
{
    private readonly OneCSendingWindow _window = new(Options.Create(new OneCSendingWindowOptions()));

    [Fact]
    public void InsideTheWindow_NothingWaits()
        // 10:00 UTC on a winter day is 12:00 in Kyiv.
        => Assert.Null(_window.NextOpeningAfter(new DateTimeOffset(2026, 11, 16, 10, 0, 0, TimeSpan.Zero)));

    [Fact]
    public void BeforeTheWindow_WaitsUntilSevenTheSameDay()
        => Assert.Equal(new DateTimeOffset(2026, 11, 16, 5, 0, 0, TimeSpan.Zero),
            _window.NextOpeningAfter(new DateTimeOffset(2026, 11, 16, 3, 30, 0, TimeSpan.Zero)));

    [Fact]
    public void AtTheEndOfTheWindow_WaitsUntilSevenTomorrow()
        // 18:00 UTC is 20:00 in Kyiv: the end is exclusive.
        => Assert.Equal(new DateTimeOffset(2026, 11, 17, 5, 0, 0, TimeSpan.Zero),
            _window.NextOpeningAfter(new DateTimeOffset(2026, 11, 16, 18, 0, 0, TimeSpan.Zero)));

    [Fact]
    public void InSummer_KyivIsThreeHoursAhead()
        => Assert.Equal(new DateTimeOffset(2026, 7, 2, 4, 0, 0, TimeSpan.Zero),
            _window.NextOpeningAfter(new DateTimeOffset(2026, 7, 1, 20, 0, 0, TimeSpan.Zero)));
}
