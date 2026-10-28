using Identity.Application.Features.Users.Shared;
using Identity.Application.Services;

namespace Identity.Application.Features.Users.CreateStaff;

public sealed class CreateStaffCommandHandler(IStaffAccountService staffAccountService)
    : ICommandHandler<CreateStaffCommand, Result<StaffAccountDto>>
{
    public async ValueTask<Result<StaffAccountDto>> Handle(CreateStaffCommand command, CancellationToken cancellationToken)
        => await staffAccountService.CreateAsync(command.Email, command.FullName, command.Role, command.Password, cancellationToken);
}
