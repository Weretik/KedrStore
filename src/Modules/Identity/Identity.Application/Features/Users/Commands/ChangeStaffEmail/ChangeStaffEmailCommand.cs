namespace Identity.Application.Features.Users.Commands.ChangeStaffEmail;

public sealed record ChangeStaffEmailCommand(Guid UserId, string Email) : ICommand<Result>;
