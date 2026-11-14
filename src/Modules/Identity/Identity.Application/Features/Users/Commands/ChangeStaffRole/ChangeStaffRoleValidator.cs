using FluentValidation;
using Identity.Application.Features.Users.Shared;

namespace Identity.Application.Features.Users.Commands.ChangeStaffRole;

public sealed class ChangeStaffRoleValidator : AbstractValidator<ChangeStaffRoleCommand>
{
    public ChangeStaffRoleValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Role).Must(StaffRoles.IsStaffRole).WithMessage("Роль може бути лише Admin, Seo або Manager.");
    }
}
