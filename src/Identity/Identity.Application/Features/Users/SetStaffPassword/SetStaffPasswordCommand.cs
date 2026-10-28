namespace Identity.Application.Features.Users.SetStaffPassword;

public sealed record SetStaffPasswordCommand(Guid UserId, string Password) : ICommand<Result>;
