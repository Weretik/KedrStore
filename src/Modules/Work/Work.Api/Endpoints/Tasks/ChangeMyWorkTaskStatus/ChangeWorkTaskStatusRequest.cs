namespace Work.Api.Endpoints.Tasks.ChangeMyWorkTaskStatus;

public sealed record ChangeWorkTaskStatusRequest(string Status, string? Result);
