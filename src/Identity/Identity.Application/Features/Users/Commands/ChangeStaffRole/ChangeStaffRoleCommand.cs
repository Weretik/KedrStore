namespace Identity.Application.Features.Users.Commands.ChangeStaffRole;

public sealed record ChangeStaffRoleCommand(Guid UserId, string Role) : ICommand<Result>;
