using FluentValidation;
using Identity.Application.Features.Users.Shared;

namespace Identity.Application.Features.Users.SetStaffPassword;

public sealed class SetStaffPasswordCommandValidator : AbstractValidator<SetStaffPasswordCommand>
{
    public SetStaffPasswordCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Password).StaffPassword();
    }
}
