using FluentValidation;
using Identity.Application.Features.Users.Shared;

namespace Identity.Application.Features.Users.Commands.SetStaffPassword;

public sealed class SetStaffPasswordValidator : AbstractValidator<SetStaffPasswordCommand>
{
    public SetStaffPasswordValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Password).StaffPassword();
    }
}
