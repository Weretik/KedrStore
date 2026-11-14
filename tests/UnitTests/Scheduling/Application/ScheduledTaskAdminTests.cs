using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Scheduling.Application.Features.Catalog;
using Scheduling.Application.Features.Tasks.Commands.RunScheduledTaskNow;
using Scheduling.Application.Features.Tasks.Commands.UpdateScheduledTaskSchedule;
using Scheduling.Application.Features.Tasks.Queries.GetScheduledTaskRuns;
using Scheduling.Application.Features.Tasks.Queries.GetScheduledTasks;
using Scheduling.Application.Features.Tasks.Shared;
using Scheduling.Domain.Schedules;

namespace UnitTests.Scheduling.Application;

/// <summary>platform/007 SC-07 (schedule), SC-08 (run now) and the page list.</summary>
public sealed class ScheduledTaskAdminTests
{
    private static readonly ScheduleInput TwiceADay = new("daily", Times: ["07:00", "13:00"], Days: [1, 2, 3, 4, 5, 6, 7]);

    private static async Task<Result<ScheduledTaskResponse>> UpdateAsync(SchedulingTestHarness harness, string key,
        ScheduledTaskScheduleInput input)
    {
        await using var db = harness.NewContext();
        return await new UpdateScheduledTaskScheduleHandler(db, harness.Time)
            .Handle(new UpdateScheduledTaskScheduleCommand(key, input, "Олена"), CancellationToken.None);
    }

    private static async Task<Result<ScheduledTaskResponse>> RunNowAsync(SchedulingTestHarness harness, string key)
    {
        await using var db = harness.NewContext();
        return await new RunScheduledTaskNowHandler(db, harness.Time)
            .Handle(new RunScheduledTaskNowCommand(key, "Олена"), CancellationToken.None);
    }

    [Fact]
    public async Task Page_lists_every_task_in_catalogue_order_with_names()
    {
        using var harness = new SchedulingTestHarness();

        var tasks = (await new GetScheduledTasksHandler(harness.Db, harness.Time)
            .Handle(new GetScheduledTasksQuery(), CancellationToken.None)).Value;

        Assert.Equal(ScheduledTaskCatalog.Tasks.Select(x => x.Key), tasks.Select(x => x.Key));
        Assert.True(tasks.All(x => !string.IsNullOrWhiteSpace(x.Name) && !string.IsNullOrWhiteSpace(x.Description)));
        Assert.False(tasks.Single(x => x.Key == ScheduledTaskKeys.Outbox).Editable);
        Assert.False(tasks.Any(x => x.LastRunFailed || x.Overdue));
    }

    [Fact]
    public async Task Admin_changes_the_schedule_and_the_next_run_follows_it()
    {
        using var harness = new SchedulingTestHarness();

        var result = await UpdateAsync(harness, ScheduledTaskKeys.Stocks, new(true, TwiceADay));

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.Enabled);
        Assert.Equal(new[] { "07:00", "13:00" }, result.Value.Schedule.Times);
        Assert.Equal(KyivTime.ToInstant(new DateTime(2026, 11, 16, 13, 0, 0)), result.Value.NextRunAt);
        var saved = await harness.NewContext().Tasks.SingleAsync(x => x.Key == ScheduledTaskKeys.Stocks);
        Assert.Equal("Олена", saved.UpdatedBy);
    }

    [Fact]
    public async Task Admin_moves_the_next_run_once()
    {
        using var harness = new SchedulingTestHarness();
        var tomorrow = harness.Time.GetUtcNow().AddDays(1);

        var moved = await UpdateAsync(harness, ScheduledTaskKeys.Stocks, new(true, TwiceADay, tomorrow));
        var past = await UpdateAsync(harness, ScheduledTaskKeys.Stocks, new(true, TwiceADay, harness.Time.GetUtcNow().AddHours(-1)));

        Assert.Equal(tomorrow, moved.Value.NextRunAt);
        Assert.Equal(ResultStatus.Invalid, past.Status);
    }

    [Fact]
    public async Task Outbox_schedule_cannot_be_changed_or_run_now()
    {
        using var harness = new SchedulingTestHarness();

        var update = await UpdateAsync(harness, ScheduledTaskKeys.Outbox, new(false, new ScheduleInput("manual")));
        var run = await RunNowAsync(harness, ScheduledTaskKeys.Outbox);

        Assert.Equal(ResultStatus.Conflict, update.Status);
        Assert.Equal(ResultStatus.Conflict, run.Status);
    }

    [Fact]
    public async Task Unknown_task_is_not_found()
    {
        using var harness = new SchedulingTestHarness();

        Assert.Equal(ResultStatus.NotFound, (await RunNowAsync(harness, "nope")).Status);
        Assert.Equal(ResultStatus.NotFound, (await UpdateAsync(harness, "nope", new(true, TwiceADay))).Status);
        var runs = await new GetScheduledTaskRunsHandler(harness.Db).Handle(new GetScheduledTaskRunsQuery("nope"), CancellationToken.None);
        Assert.Equal(ResultStatus.NotFound, runs.Status);
    }

    [Fact]
    public async Task Run_now_starts_a_disabled_task_on_the_next_tick_once()
    {
        using var harness = new SchedulingTestHarness();

        var requested = await RunNowAsync(harness, ScheduledTaskKeys.Stocks);
        var again = await RunNowAsync(harness, ScheduledTaskKeys.Stocks);
        var tick = await harness.TickAsync();
        var whileRunning = await RunNowAsync(harness, ScheduledTaskKeys.Stocks);

        Assert.Equal(harness.Time.GetUtcNow(), requested.Value.RunRequestedAt);
        Assert.Equal(ResultStatus.Conflict, again.Status);
        Assert.Equal(new[] { ScheduledTaskKeys.Stocks }, tick.Started);
        Assert.Equal(ResultStatus.Conflict, whileRunning.Status);
        Assert.Contains(whileRunning.Errors, error => error.Contains("вже виконується"));

        var runs = (await new GetScheduledTaskRunsHandler(harness.Db)
            .Handle(new GetScheduledTaskRunsQuery(ScheduledTaskKeys.Stocks), CancellationToken.None)).Value;
        Assert.Equal("manual", runs.Single().Trigger);
        Assert.Equal("Олена", runs.Single().RequestedBy);
    }
}
