namespace Identity.Application.Features.Profile.Commands.ChangeOwnPassword;

public sealed record ChangeOwnPasswordCommand(string CurrentPassword, string NewPassword) : ICommand<Result>;
