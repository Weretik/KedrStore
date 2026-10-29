using FluentValidation;
using Identity.Application.Features.Profile.Shared;

namespace Identity.Application.Features.Users.ChangeStaffEmail;

public sealed class ChangeStaffEmailCommandValidator : AbstractValidator<ChangeStaffEmailCommand>
{
    public ChangeStaffEmailCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Email).StaffEmail();
    }
}
