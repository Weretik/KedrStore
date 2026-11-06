namespace Work.Application.Features.Tasks.Common;

public sealed record WorkTaskDto(
    long Id,
    Guid OwnerUserId,
    Guid CreatedByUserId,
    bool CreatedByOwner,
    string Title,
    string? Details,
    string Status,
    string? Result,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt)
{
    public static WorkTaskDto From(WorkTask task) => new(
        task.Id.Value,
        task.OwnerUserId,
        task.CreatedByUserId,
        task.IsCreatedByOwner,
        task.Title,
        task.Details,
        task.Status.ToString(),
        task.Result,
        task.CreatedAt,
        task.UpdatedAt,
        task.StartedAt,
        task.CompletedAt);
}
