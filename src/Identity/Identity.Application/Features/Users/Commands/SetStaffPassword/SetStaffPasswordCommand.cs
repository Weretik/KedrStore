namespace Identity.Application.Features.Users.Commands.SetStaffPassword;

public sealed record SetStaffPasswordCommand(Guid UserId, string Password) : ICommand<Result>;
