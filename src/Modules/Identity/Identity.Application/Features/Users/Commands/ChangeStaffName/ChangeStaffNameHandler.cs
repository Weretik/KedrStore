using Identity.Application.Abstractions.Accounts;
using Identity.Application.Abstractions.Sessions;

namespace Identity.Application.Features.Users.Commands.ChangeStaffName;

public sealed class ChangeStaffNameHandler(IStaffProfileEditService staffProfileEditService)
    : ICommandHandler<ChangeStaffNameCommand, Result>
{
    public async ValueTask<Result> Handle(ChangeStaffNameCommand command, CancellationToken cancellationToken)
        => await staffProfileEditService.ChangeNameAsync(command.UserId, command.FullName, cancellationToken);
}
