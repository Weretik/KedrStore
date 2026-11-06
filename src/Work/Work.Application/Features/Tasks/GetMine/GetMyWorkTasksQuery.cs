using Work.Application.Features.Tasks.Common;

namespace Work.Application.Features.Tasks.GetMine;

/// <summary>The signed-in staff member's board: open tasks and tasks done in the last 30 days.</summary>
public sealed record GetMyWorkTasksQuery : IQuery<Result<IReadOnlyList<WorkTaskDto>>>;
