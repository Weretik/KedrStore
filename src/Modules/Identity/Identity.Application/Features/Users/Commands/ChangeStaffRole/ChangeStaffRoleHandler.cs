using Identity.Application.Abstractions.Accounts;
using Identity.Application.Abstractions.Sessions;

namespace Identity.Application.Features.Users.Commands.ChangeStaffRole;

public sealed class ChangeStaffRoleHandler(IStaffAccountService staffAccountService)
    : ICommandHandler<ChangeStaffRoleCommand, Result>
{
    public async ValueTask<Result> Handle(ChangeStaffRoleCommand command, CancellationToken cancellationToken)
        => await staffAccountService.ChangeRoleAsync(command.UserId, command.Role, cancellationToken);
}
