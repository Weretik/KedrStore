using Scheduling.Application.Features.Tasks.Shared;

namespace Scheduling.Application.Features.Tasks.Queries.GetScheduledTasks;

public sealed record GetScheduledTasksQuery : IQuery<Result<IReadOnlyList<ScheduledTaskResponse>>>;
