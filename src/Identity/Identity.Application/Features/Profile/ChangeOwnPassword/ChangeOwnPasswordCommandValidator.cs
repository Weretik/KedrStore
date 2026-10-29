using FluentValidation;
using Identity.Application.Features.Users.Shared;

namespace Identity.Application.Features.Profile.ChangeOwnPassword;

public sealed class ChangeOwnPasswordCommandValidator : AbstractValidator<ChangeOwnPasswordCommand>
{
    public ChangeOwnPasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword).NotEmpty();
        RuleFor(x => x.NewPassword).StaffPassword();
    }
}
