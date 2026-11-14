using Identity.Application.Features.Users.Shared;

namespace Identity.Application.Features.Users.Commands.CreateStaff;

public sealed record CreateStaffCommand(string Email, string FullName, string Role, string Password)
    : ICommand<Result<StaffAccountDto>>;
