using FluentValidation;
using Identity.Application.Features.Users.Shared;

namespace Identity.Application.Features.Profile.Commands.ChangeOwnPassword;

public sealed class ChangeOwnPasswordValidator : AbstractValidator<ChangeOwnPasswordCommand>
{
    public ChangeOwnPasswordValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword).StaffPassword();
    }
}
