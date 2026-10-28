using Identity.Application.Services;

namespace Identity.Application.Features.Users.SetStaffBlocked;

public sealed class SetStaffBlockedCommandHandler(IStaffAccountService staffAccountService)
    : ICommandHandler<SetStaffBlockedCommand, Result>
{
    public async ValueTask<Result> Handle(SetStaffBlockedCommand command, CancellationToken cancellationToken)
        => await staffAccountService.SetBlockedAsync(command.UserId, command.Blocked, cancellationToken);
}
