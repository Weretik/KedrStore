namespace Work.Application.Features.Activity.Record;

/// <summary>R-5: written by the activity filter after a successful admin change; never sent by a client.</summary>
public sealed record RecordWorkActivityCommand(
    Guid UserId,
    string? UserEmail,
    string Area,
    string Operation,
    string Method,
    string? Target) : ICommand<Result>;
