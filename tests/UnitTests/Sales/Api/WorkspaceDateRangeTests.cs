using Sales.Api.Controllers;
using Sales.Application.Contracts.Orders;

namespace UnitTests.Sales.Api;

public sealed class WorkspaceDateRangeTests
{
    [Theory]
    [InlineData("2026-3-09", null)]
    [InlineData("09-03-2026", null)]
    [InlineData("2026-02-30", null)]
    [InlineData(null, "2026-03-9")]
    public void Parse_RejectsDatesOutsideExactIsoFormat(string? from, string? to)
    {
        var exception = Assert.Throws<WorkspaceException>(() => WorkspaceDateRange.Parse(from, to));

        Assert.Equal(400, exception.StatusCode);
        Assert.Equal("invalid_date_range", exception.Code);
    }

    [Fact]
    public void Parse_RejectsReversedRange()
    {
        var exception = Assert.Throws<WorkspaceException>(() =>
            WorkspaceDateRange.Parse("2026-03-10", "2026-03-09"));

        Assert.Equal("invalid_date_range", exception.Code);
    }

    [Fact]
    public void Parse_UsesInclusiveKyivCalendarDayWithExclusiveNextMidnight()
    {
        var range = WorkspaceDateRange.Parse("2026-01-15", "2026-01-15");

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
        var range = WorkspaceDateRange.Parse(date, date);

        Assert.Equal(DateTimeOffset.Parse(expectedFromUtc), range.FromUtc);
        Assert.Equal(DateTimeOffset.Parse(expectedToUtc), range.ToUtcExclusive);
        Assert.Equal(TimeSpan.FromHours(expectedHours), range.ToUtcExclusive - range.FromUtc);
    }
}
