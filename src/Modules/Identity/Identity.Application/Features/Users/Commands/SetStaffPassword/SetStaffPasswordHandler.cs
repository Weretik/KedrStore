using Identity.Application.Abstractions.Accounts;
using Identity.Application.Abstractions.Sessions;

namespace Identity.Application.Features.Users.Commands.SetStaffPassword;

public sealed class SetStaffPasswordHandler(IStaffAccountService staffAccountService)
    : ICommandHandler<SetStaffPasswordCommand, Result>
{
    public async ValueTask<Result> Handle(SetStaffPasswordCommand command, CancellationToken cancellationToken)
        => await staffAccountService.SetPasswordAsync(command.UserId, command.Password, cancellationToken);
}
