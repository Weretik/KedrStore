using Scheduling.Application.Features.Tasks.Shared;

namespace Scheduling.Application.Features.Tasks.Queries.GetScheduledTaskRuns;

public sealed record GetScheduledTaskRunsQuery(string Key) : IQuery<Result<IReadOnlyList<ScheduledTaskRunResponse>>>;
