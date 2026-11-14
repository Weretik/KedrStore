using Scheduling.Application.Features.Catalog;
using Scheduling.Application.Features.Tasks.Shared;

namespace Scheduling.Application.Features.Tasks.Queries.GetScheduledTaskRuns;

/// <summary>The last 50 runs of a task, newest first (platform/007 R-09).</summary>
public sealed class GetScheduledTaskRunsHandler(ISchedulingDbContext db)
    : IQueryHandler<GetScheduledTaskRunsQuery, Result<IReadOnlyList<ScheduledTaskRunResponse>>>
{
    public const int Limit = 50;

    public async ValueTask<Result<IReadOnlyList<ScheduledTaskRunResponse>>> Handle(GetScheduledTaskRunsQuery query,
        CancellationToken cancellationToken)
    {
        if (ScheduledTaskCatalog.Find(query.Key) is null) return Result<IReadOnlyList<ScheduledTaskRunResponse>>.NotFound();
        var runs = await db.Runs.AsNoTracking().Where(x => x.TaskKey == query.Key)
            .OrderByDescending(x => x.ClaimedAt).Take(Limit).ToListAsync(cancellationToken);
        return Result.Success<IReadOnlyList<ScheduledTaskRunResponse>>(runs.Select(ScheduledTaskMapper.ToResponse).ToArray());
    }
}
