namespace Identity.Application.Features.Profile.ChangeOwnName;

public sealed record ChangeOwnNameCommand(string FullName) : ICommand<Result>;
