using Work.Application.Features.Tasks.Shared;

namespace Work.Application.Features.Tasks.Queries.GetMyWorkTasks;

/// <summary>The signed-in staff member's board: open tasks and tasks done in the last 30 days.</summary>
public sealed record GetMyWorkTasksQuery : IQuery<Result<IReadOnlyList<WorkTaskDto>>>;
