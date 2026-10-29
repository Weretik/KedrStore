using Identity.Application.Services;

namespace Identity.Application.Features.Users.ChangeStaffEmail;

public sealed class ChangeStaffEmailCommandHandler(IStaffProfileEditService staffProfileEditService)
    : ICommandHandler<ChangeStaffEmailCommand, Result>
{
    public async ValueTask<Result> Handle(ChangeStaffEmailCommand command, CancellationToken cancellationToken)
        => await staffProfileEditService.ChangeEmailAsync(command.UserId, command.Email, cancellationToken);
}
