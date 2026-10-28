using FluentValidation;
using Identity.Application.Features.Users.Shared;

namespace Identity.Application.Features.Users.ChangeStaffRole;

public sealed class ChangeStaffRoleCommandValidator : AbstractValidator<ChangeStaffRoleCommand>
{
    public ChangeStaffRoleCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Role).Must(StaffRoles.IsStaffRole).WithMessage("Role must be Admin or Manager.");
    }
}
