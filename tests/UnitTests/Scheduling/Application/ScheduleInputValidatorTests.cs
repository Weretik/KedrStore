using Scheduling.Application.Features.Tasks.Shared;

namespace UnitTests.Scheduling.Application;

/// <summary>Admin limits of a schedule (platform/007 R-02): Ukrainian messages, at least every 5 minutes.</summary>
public sealed class ScheduleInputValidatorTests
{
    private static readonly ScheduleInputValidator Validator = new();

    [Theory]
    [InlineData("interval", 5, null, null)]
    [InlineData("interval", 60, 8, 20)]
    [InlineData("manual", null, null, null)]
    public void Valid_intervals_and_manual_pass(string kind, int? every, int? from, int? to) =>
        Assert.True(Validator.Validate(new ScheduleInput(kind, every, from, to)).IsValid);

    [Theory]
    [InlineData("interval", 1, null, null)]
    [InlineData("interval", 60, 8, null)]
    [InlineData("interval", 60, 20, 8)]
    [InlineData("weekly", null, null, null)]
    public void Invalid_intervals_fail(string kind, int? every, int? from, int? to) =>
        Assert.False(Validator.Validate(new ScheduleInput(kind, every, from, to)).IsValid);

    [Fact]
    public void Daily_needs_valid_distinct_times_and_days()
    {
        Assert.True(Validator.Validate(new ScheduleInput("daily", Times: ["07:00", "13:00"], Days: [1, 5])).IsValid);
        Assert.False(Validator.Validate(new ScheduleInput("daily", Times: ["7"], Days: [1])).IsValid);
        Assert.False(Validator.Validate(new ScheduleInput("daily", Times: ["07:00", "07:00"], Days: [1])).IsValid);
        Assert.False(Validator.Validate(new ScheduleInput("daily", Times: ["07:00"], Days: [])).IsValid);
        Assert.False(Validator.Validate(new ScheduleInput("daily", Times: ["07:00"], Days: [8])).IsValid);
        var error = Validator.Validate(new ScheduleInput("daily", Times: [], Days: [1])).Errors.Single();
        Assert.StartsWith("Вкажіть", error.ErrorMessage);
    }
}
