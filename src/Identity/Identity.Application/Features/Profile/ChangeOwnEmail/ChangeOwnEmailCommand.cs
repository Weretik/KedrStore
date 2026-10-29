namespace Identity.Application.Features.Profile.ChangeOwnEmail;

public sealed record ChangeOwnEmailCommand(string Email, string CurrentPassword) : ICommand<Result>;
