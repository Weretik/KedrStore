namespace Identity.Application.Features.Users.ChangeStaffRole;

public sealed record ChangeStaffRoleCommand(Guid UserId, string Role) : ICommand<Result>;
