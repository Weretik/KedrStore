namespace Work.Api.Requests;

public sealed record SaveWorkTaskRequest(string Title, string? Details);

public sealed record ChangeWorkTaskStatusRequest(string Status, string? Result);

public sealed record CreateWorkTaskForUserRequest(Guid OwnerUserId, string Title, string? Details);
