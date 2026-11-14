using Work.Application.Features.Tasks.Shared;

namespace Work.Application.Features.Tasks.Commands.CreateMyWorkTask;

/// <summary>A task the signed-in staff member sets for themselves; it starts in «План».</summary>
public sealed record CreateMyWorkTaskCommand(
    string Title, string? Details, DateOnly? DueDate, IReadOnlyList<WorkTaskStepInput>? Steps)
    : ICommand<Result<WorkTaskDto>>;
