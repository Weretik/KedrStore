using FluentValidation;
using Identity.Application.Features.Profile.Shared;

namespace Identity.Application.Features.Profile.ChangeOwnEmail;

public sealed class ChangeOwnEmailCommandValidator : AbstractValidator<ChangeOwnEmailCommand>
{
    public ChangeOwnEmailCommandValidator()
    {
        RuleFor(x => x.Email).StaffEmail();
        RuleFor(x => x.CurrentPassword).NotEmpty();
    }
}
