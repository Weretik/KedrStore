using Identity.Application.Services;

namespace Identity.Application.Features.Users.SetStaffPassword;

public sealed class SetStaffPasswordCommandHandler(IStaffAccountService staffAccountService)
    : ICommandHandler<SetStaffPasswordCommand, Result>
{
    public async ValueTask<Result> Handle(SetStaffPasswordCommand command, CancellationToken cancellationToken)
        => await staffAccountService.SetPasswordAsync(command.UserId, command.Password, cancellationToken);
}
