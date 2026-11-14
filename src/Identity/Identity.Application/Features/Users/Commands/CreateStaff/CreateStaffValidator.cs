using FluentValidation;
using Identity.Application.Features.Users.Shared;

namespace Identity.Application.Features.Users.Commands.CreateStaff;

public sealed class CreateStaffValidator : AbstractValidator<CreateStaffCommand>
{
    public CreateStaffValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Role).Must(StaffRoles.IsStaffRole).WithMessage("Роль може бути лише Admin, Seo або Manager.");
        RuleFor(x => x.Password).StaffPassword();
    }
}
