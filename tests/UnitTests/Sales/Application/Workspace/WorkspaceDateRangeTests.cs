using Sales.Application.Features.OrderWorkspace.Shared;
namespace UnitTests.Sales.Application.Workspace;

public sealed class WorkspaceDateRangeTests
{
    [Theory]
    [InlineData("2026-3-09", null)]
    [InlineData("09-03-2026", null)]
    [InlineData("2026-02-30", null)]
    [InlineData(null, "2026-03-9")]
    public void Parse_RejectsDatesOutsideExactIsoFormat(string? from, string? to)
    {
        var result = WorkspaceDateRange.Parse(from, to);

        Assert.Equal("invalid_date_range", result.Error?.Code);
    }

    [Fact]
    public void Parse_RejectsReversedRange()
    {
        var result = WorkspaceDateRange.Parse("2026-03-10", "2026-03-09");

        Assert.Equal("invalid_date_range", result.Error?.Code);
    }

    [Fact]
    public void Parse_UsesInclusiveKyivCalendarDayWithExclusiveNextMidnight()
    {
        var range = WorkspaceDateRange.Parse("2026-01-15", "2026-01-15").Value;

        Assert.Equal(DateTimeOffset.Parse("2026-01-14T22:00:00Z"), range.FromUtc);
        Assert.Equal(DateTimeOffset.Parse("2026-01-15T22:00:00Z"), range.ToUtcExclusive);
    }

    [Theory]
    [InlineData("2026-03-29", "2026-03-28T22:00:00Z", "2026-03-29T21:00:00Z", 23)]
    [InlineData("2026-10-25", "2026-10-24T21:00:00Z", "2026-10-25T22:00:00Z", 25)]
    public void Parse_UsesKyivOffsetForEachMidnightAcrossDst(
        string date,
        string expectedFromUtc,
        string expectedToUtc,
        int expectedHours)
    {
        var range = WorkspaceDateRange.Parse(date, date).Value;

        Assert.Equal(DateTimeOffset.Parse(expectedFromUtc), range.FromUtc);
        Assert.Equal(DateTimeOffset.Parse(expectedToUtc), range.ToUtcExclusive);
        Assert.Equal(TimeSpan.FromHours(expectedHours), range.ToUtcExclusive - range.FromUtc);
    }

    [Theory]
    [InlineData("2026-11-05T01:00:00Z", "2026-11-04T22:00:00Z")]
    [InlineData("2026-11-04T21:59:00Z", "2026-11-03T22:00:00Z")]
    [InlineData("2026-07-01T12:00:00Z", "2026-06-30T21:00:00Z")]
    public void TodayStartUtc_IsTheCurrentKyivMidnight(string now, string expected)
        => Assert.Equal(DateTimeOffset.Parse(expected), WorkspaceDateRange.TodayStartUtc(DateTimeOffset.Parse(now)));
}
