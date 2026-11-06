using Work.Application.Features.Tasks.Common;

namespace Work.Application.Features.Tasks.Create;

/// <summary>A task the signed-in staff member sets for themselves; it starts in «План».</summary>
public sealed record CreateMyWorkTaskCommand(
    string Title, string? Details, DateOnly? DueDate, IReadOnlyList<WorkTaskStepInput>? Steps)
    : ICommand<Result<WorkTaskDto>>;
