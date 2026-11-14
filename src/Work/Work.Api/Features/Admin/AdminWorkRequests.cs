using Work.Domain.Tasks;

namespace Work.Api.Features.Admin;

public sealed record CreateWorkTaskForUserRequest(
    Guid OwnerUserId, string Title, string? Details, DateOnly? DueDate = null,
    IReadOnlyList<WorkTaskStepInput>? Steps = null);
