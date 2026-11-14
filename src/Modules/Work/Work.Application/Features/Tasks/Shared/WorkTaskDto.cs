namespace Work.Application.Features.Tasks.Shared;

public sealed record WorkTaskStepDto(string Title, bool IsDone);

public sealed record WorkTaskDto(
    long Id,
    Guid OwnerUserId,
    Guid CreatedByUserId,
    bool CreatedByOwner,
    string Title,
    string? Details,
    string Status,
    string? Result,
    DateOnly? DueDate,
    bool IsOverdue,
    IReadOnlyList<WorkTaskStepDto> Steps,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt)
{
    /// <param name="today">Kyiv day the overdue mark is computed for.</param>
    public static WorkTaskDto From(WorkTask task, DateOnly today) => new(
        task.Id.Value,
        task.OwnerUserId,
        task.CreatedByUserId,
        task.IsCreatedByOwner,
        task.Title,
        task.Details,
        task.Status.ToString(),
        task.Result,
        task.DueDate,
        task.IsOverdue(today),
        task.Steps.Select(step => new WorkTaskStepDto(step.Title, step.IsDone)).ToList(),
        task.CreatedAt,
        task.UpdatedAt,
        task.StartedAt,
        task.CompletedAt);
}
