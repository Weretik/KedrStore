using FluentValidation;

namespace Identity.Application.Features.Users.SetStaffBlocked;

public sealed class SetStaffBlockedCommandValidator : AbstractValidator<SetStaffBlockedCommand>
{
    public SetStaffBlockedCommandValidator() => RuleFor(x => x.UserId).NotEmpty();
}
