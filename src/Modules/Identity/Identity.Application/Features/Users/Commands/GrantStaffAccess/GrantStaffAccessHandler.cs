using Identity.Application.Features.Users.Shared;
using Identity.Application.Abstractions.Accounts;
using Identity.Application.Abstractions.Sessions;

namespace Identity.Application.Features.Users.Commands.GrantStaffAccess;

public sealed class GrantStaffAccessHandler(IStaffAccountService staffAccountService)
    : ICommandHandler<GrantStaffAccessCommand, Result<StaffAccountDto>>
{
    public async ValueTask<Result<StaffAccountDto>> Handle(GrantStaffAccessCommand command, CancellationToken cancellationToken)
        => await staffAccountService.GrantAccessAsync(
            command.UserId,
            command.FullName,
            command.Role,
            command.Password,
            cancellationToken);
}
