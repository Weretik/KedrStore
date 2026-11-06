using Work.Application.Features.Tasks.Common;

namespace Work.Application.Features.Tasks.CreateForUser;

/// <summary>R-4: an administrator puts a task on a staff member's board.</summary>
public sealed record CreateWorkTaskForUserCommand(
    Guid OwnerUserId, string Title, string? Details, DateOnly? DueDate, IReadOnlyList<WorkTaskStepInput>? Steps)
    : ICommand<Result<WorkTaskDto>>;
