using FluentValidation;
using Identity.Application.Features.Profile.Shared;

namespace Identity.Application.Features.Users.Commands.ChangeStaffEmail;

public sealed class ChangeStaffEmailValidator : AbstractValidator<ChangeStaffEmailCommand>
{
    public ChangeStaffEmailValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Email).StaffEmail();
    }
}
