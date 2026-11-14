using FluentValidation;
using Identity.Application.Features.Profile.Shared;

namespace Identity.Application.Features.Users.Commands.ChangeStaffName;

public sealed class ChangeStaffNameValidator : AbstractValidator<ChangeStaffNameCommand>
{
    public ChangeStaffNameValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.FullName).FullName();
    }
}
