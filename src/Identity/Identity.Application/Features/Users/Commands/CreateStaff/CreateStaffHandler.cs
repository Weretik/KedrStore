using Identity.Application.Features.Users.Shared;
using Identity.Application.Abstractions.Accounts;
using Identity.Application.Abstractions.Sessions;

namespace Identity.Application.Features.Users.Commands.CreateStaff;

public sealed class CreateStaffHandler(IStaffAccountService staffAccountService)
    : ICommandHandler<CreateStaffCommand, Result<StaffAccountDto>>
{
    public async ValueTask<Result<StaffAccountDto>> Handle(CreateStaffCommand command, CancellationToken cancellationToken)
        => await staffAccountService.CreateAsync(command.Email, command.FullName, command.Role, command.Password, cancellationToken);
}
