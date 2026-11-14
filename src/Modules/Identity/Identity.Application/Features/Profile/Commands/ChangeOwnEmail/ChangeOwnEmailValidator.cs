using FluentValidation;
using Identity.Application.Features.Profile.Shared;

namespace Identity.Application.Features.Profile.Commands.ChangeOwnEmail;

public sealed class ChangeOwnEmailValidator : AbstractValidator<ChangeOwnEmailCommand>
{
    public ChangeOwnEmailValidator()
    {
        RuleFor(x => x.Email).StaffEmail();
        RuleFor(x => x.CurrentPassword).NotEmpty();
    }
}
