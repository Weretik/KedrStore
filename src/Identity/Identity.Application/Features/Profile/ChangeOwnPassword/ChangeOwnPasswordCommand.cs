namespace Identity.Application.Features.Profile.ChangeOwnPassword;

public sealed record ChangeOwnPasswordCommand(string CurrentPassword, string NewPassword) : ICommand<Result>;
