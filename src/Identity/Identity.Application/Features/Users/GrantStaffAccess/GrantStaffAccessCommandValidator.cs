using FluentValidation;
using Identity.Application.Features.Users.Shared;

namespace Identity.Application.Features.Users.GrantStaffAccess;

public sealed class GrantStaffAccessCommandValidator : AbstractValidator<GrantStaffAccessCommand>
{
    public GrantStaffAccessCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Role).Must(StaffRoles.IsStaffRole).WithMessage("Role must be Admin, Seo or Manager.");
        RuleFor(x => x.Password).StaffPassword();
    }
}
