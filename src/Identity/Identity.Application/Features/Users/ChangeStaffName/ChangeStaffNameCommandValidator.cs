using FluentValidation;
using Identity.Application.Features.Profile.Shared;

namespace Identity.Application.Features.Users.ChangeStaffName;

public sealed class ChangeStaffNameCommandValidator : AbstractValidator<ChangeStaffNameCommand>
{
    public ChangeStaffNameCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.FullName).FullName();
    }
}
