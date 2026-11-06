using Work.Application.Features.Tasks.Common;

namespace Work.Application.Features.Tasks.ChangeStatus;

/// <summary>
/// Moves an own task to another board column (<c>Planned</c>, <c>InProgress</c>, <c>Done</c>);
/// <paramref name="Result"/> is kept only for <c>Done</c>.
/// </summary>
public sealed record ChangeMyWorkTaskStatusCommand(long Id, string Status, string? Result)
    : ICommand<Result<WorkTaskDto>>;
