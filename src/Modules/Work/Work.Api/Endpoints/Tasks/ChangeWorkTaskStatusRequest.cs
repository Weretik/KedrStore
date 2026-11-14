namespace Work.Api.Endpoints.Tasks;

public sealed record ChangeWorkTaskStatusRequest(string Status, string? Result);
