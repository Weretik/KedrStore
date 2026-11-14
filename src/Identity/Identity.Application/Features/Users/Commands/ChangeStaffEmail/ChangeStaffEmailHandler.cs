using Identity.Application.Abstractions.Accounts;
using Identity.Application.Abstractions.Sessions;

namespace Identity.Application.Features.Users.Commands.ChangeStaffEmail;

public sealed class ChangeStaffEmailHandler(IStaffProfileEditService staffProfileEditService)
    : ICommandHandler<ChangeStaffEmailCommand, Result>
{
    public async ValueTask<Result> Handle(ChangeStaffEmailCommand command, CancellationToken cancellationToken)
        => await staffProfileEditService.ChangeEmailAsync(command.UserId, command.Email, cancellationToken);
}
