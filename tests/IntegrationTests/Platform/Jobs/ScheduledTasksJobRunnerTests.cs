extern alias JobsHost;

using ScheduledTaskRunners = JobsHost::Host.Jobs.Tasks.ScheduledTaskRunners;
using ScheduledTasksJobRunner = JobsHost::Host.Jobs.Tasks.ScheduledTasksJobRunner;
using Scheduling.Application.Features.Catalog;

namespace IntegrationTests.Platform.Jobs;

/// <summary>platform/007 R-06: the job's argument and a runner for every task the job runs.</summary>
public sealed class ScheduledTasksJobRunnerTests
{
    [Fact]
    public void Parse_reads_key_and_run_pairs()
    {
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();

        var parsed = ScheduledTasksJobRunner.Parse($"stocks:{first}, prices:{second}");

        Assert.Equal([("stocks", first), ("prices", second)], parsed);
        Assert.Empty(ScheduledTasksJobRunner.Parse(null));
        Assert.Throws<ArgumentException>(() => ScheduledTasksJobRunner.Parse("stocks"));
    }

    [Fact]
    public void Every_job_task_of_the_catalogue_has_a_runner()
    {
        var jobTasks = ScheduledTaskCatalog.Tasks.Where(x => !x.RunsInTick).Select(x => x.Key);

        Assert.Equal(jobTasks.Order(), ScheduledTaskRunners.All.Select(x => x.Key).Order());
    }
}
