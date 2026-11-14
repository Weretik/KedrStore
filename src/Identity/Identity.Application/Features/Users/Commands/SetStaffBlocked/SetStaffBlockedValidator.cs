using FluentValidation;

namespace Identity.Application.Features.Users.Commands.SetStaffBlocked;

public sealed class SetStaffBlockedValidator : AbstractValidator<SetStaffBlockedCommand>
{
    public SetStaffBlockedValidator() => RuleFor(x => x.UserId).NotEmpty();
}
