using Identity.Application.Services;

namespace Identity.Application.Features.Users.ChangeStaffRole;

public sealed class ChangeStaffRoleCommandHandler(IStaffAccountService staffAccountService)
    : ICommandHandler<ChangeStaffRoleCommand, Result>
{
    public async ValueTask<Result> Handle(ChangeStaffRoleCommand command, CancellationToken cancellationToken)
        => await staffAccountService.ChangeRoleAsync(command.UserId, command.Role, cancellationToken);
}
