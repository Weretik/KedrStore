namespace Scheduling.Application.Features.Tick.Commands.RunSchedulerTick;

/// <summary>One minute of the scheduler (platform/007 R-04): Cloud Scheduler calls it through the protected tick endpoint.</summary>
public sealed record RunSchedulerTickCommand : ICommand<Result<SchedulerTickResponse>>;

/// <summary>What the tick did: the inline work's results and the tasks it started.</summary>
public sealed record SchedulerTickResponse(IReadOnlyDictionary<string, string> Inline, IReadOnlyList<string> Started,
    string? LaunchError);
