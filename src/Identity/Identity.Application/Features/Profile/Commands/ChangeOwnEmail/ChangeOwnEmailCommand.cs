namespace Identity.Application.Features.Profile.Commands.ChangeOwnEmail;

public sealed record ChangeOwnEmailCommand(string Email, string CurrentPassword) : ICommand<Result>;
