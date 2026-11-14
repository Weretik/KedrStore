using Work.Application.Features.Tasks.Shared;

namespace Work.Application.Features.Tasks.Commands.CreateWorkTaskForUser;

/// <summary>R-4: an administrator puts a task on a staff member's board.</summary>
public sealed record CreateWorkTaskForUserCommand(
    Guid OwnerUserId, string Title, string? Details, DateOnly? DueDate, IReadOnlyList<WorkTaskStepInput>? Steps)
    : ICommand<Result<WorkTaskDto>>;
