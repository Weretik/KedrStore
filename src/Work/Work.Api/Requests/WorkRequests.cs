using Work.Domain.Tasks;

namespace Work.Api.Requests;

/// <param name="DueDate">Day the task should be done by; null means no deadline.</param>
/// <param name="Steps">The whole list of steps in order; null or empty means none.</param>
public sealed record SaveWorkTaskRequest(
    string Title, string? Details, DateOnly? DueDate = null, IReadOnlyList<WorkTaskStepInput>? Steps = null);

public sealed record ChangeWorkTaskStatusRequest(string Status, string? Result);

public sealed record SetWorkTaskStepRequest(bool IsDone);

public sealed record CreateWorkTaskForUserRequest(
    Guid OwnerUserId, string Title, string? Details, DateOnly? DueDate = null,
    IReadOnlyList<WorkTaskStepInput>? Steps = null);
