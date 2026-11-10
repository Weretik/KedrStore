using Identity.Application.Features.Users.Shared;
using Identity.Application.Services;

namespace Identity.Application.Features.Users.GrantStaffAccess;

public sealed class GrantStaffAccessCommandHandler(IStaffAccountService staffAccountService)
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
