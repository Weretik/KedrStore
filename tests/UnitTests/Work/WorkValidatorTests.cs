using Work.Application.Features.Report.GetReport;
using Work.Application.Features.Tasks.ChangeStatus;

namespace UnitTests.Work;

public sealed class WorkValidatorTests
{
    [Theory]
    [InlineData("Planned", true)]
    [InlineData("Done", true)]
    [InlineData("done", false)]
    [InlineData("2", false)]
    public void Status_Is_Spelled_By_Name(string status, bool valid) =>
        Assert.Equal(valid, new ChangeMyWorkTaskStatusCommandValidator()
            .Validate(new ChangeMyWorkTaskStatusCommand(1, status, null)).IsValid);

    [Fact]
    public void Report_Period_Is_At_Most_92_Days_And_Ordered()
    {
        var validator = new GetWorkReportQueryValidator();
        var from = new DateOnly(2026, 8, 1);

        Assert.True(validator.Validate(new GetWorkReportQuery(null, from, from.AddDays(91))).IsValid);
        Assert.False(validator.Validate(new GetWorkReportQuery(null, from, from.AddDays(92))).IsValid);
        Assert.False(validator.Validate(new GetWorkReportQuery(null, from, from.AddDays(-1))).IsValid);
    }

    [Fact]
    public void Report_Days_Start_At_Kyiv_Midnight()
    {
        // 2026-11-06 is winter time in Kyiv (UTC+2).
        Assert.Equal(new DateTimeOffset(2026, 11, 5, 22, 0, 0, TimeSpan.Zero),
            GetWorkReportQueryHandler.KyivMidnightUtc(new DateOnly(2026, 11, 6)));
    }
}
