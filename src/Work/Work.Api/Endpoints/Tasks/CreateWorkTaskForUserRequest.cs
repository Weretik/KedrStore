using Work.Domain.Tasks;

namespace Work.Api.Endpoints.Tasks;

public sealed record CreateWorkTaskForUserRequest(
    Guid OwnerUserId, string Title, string? Details, DateOnly? DueDate = null,
    IReadOnlyList<WorkTaskStepInput>? Steps = null);
