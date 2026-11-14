namespace Identity.Application.Features.Profile.Commands.ChangeOwnName;

public sealed record ChangeOwnNameCommand(string FullName) : ICommand<Result>;
