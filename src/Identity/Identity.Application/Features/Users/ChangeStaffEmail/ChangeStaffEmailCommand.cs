namespace Identity.Application.Features.Users.ChangeStaffEmail;

public sealed record ChangeStaffEmailCommand(Guid UserId, string Email) : ICommand<Result>;
