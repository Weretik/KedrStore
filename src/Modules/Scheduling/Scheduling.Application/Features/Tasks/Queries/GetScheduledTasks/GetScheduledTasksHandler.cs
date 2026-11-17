using Scheduling.Application.Features.Tasks.Shared;

namespace Scheduling.Application.Features.Tasks.Queries.GetScheduledTasks;

/// <summary>Every catalogue task with its schedule and last results; missing rows are shown with the defaults.
/// The query writes nothing; the scheduler tick saves new and changed rows.</summary>
public sealed class GetScheduledTasksHandler(ISchedulingDbContext db, TimeProvider clock)
    : IQueryHandler<GetScheduledTasksQuery, Result<IReadOnlyList<ScheduledTaskResponse>>>
{
    public async ValueTask<Result<IReadOnlyList<ScheduledTaskResponse>>> Handle(GetScheduledTasksQuery query,
        CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var rows = await ScheduledTaskRows.ReadAsync(db, now, cancellationToken);
        return Result.Success<IReadOnlyList<ScheduledTaskResponse>>(
            rows.Select(x => ScheduledTaskMapper.ToResponse(x.Definition, x.Task, now)).ToArray());
    }
}
