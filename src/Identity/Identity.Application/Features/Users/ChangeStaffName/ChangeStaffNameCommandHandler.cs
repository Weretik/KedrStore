using Identity.Application.Services;

namespace Identity.Application.Features.Users.ChangeStaffName;

public sealed class ChangeStaffNameCommandHandler(IStaffProfileEditService staffProfileEditService)
    : ICommandHandler<ChangeStaffNameCommand, Result>
{
    public async ValueTask<Result> Handle(ChangeStaffNameCommand command, CancellationToken cancellationToken)
        => await staffProfileEditService.ChangeNameAsync(command.UserId, command.FullName, cancellationToken);
}
