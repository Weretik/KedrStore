using Identity.Application.Features.Users.Shared;

namespace Identity.Application.Features.Users.Commands.GrantStaffAccess;

/// <summary>
/// Gives an existing non-staff account (usually a customer) a staff role and a new password.
/// The account keeps its customer role and data.
/// </summary>
public sealed record GrantStaffAccessCommand(Guid UserId, string FullName, string Role, string Password)
    : ICommand<Result<StaffAccountDto>>;
