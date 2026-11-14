using FluentValidation;
using Identity.Application.Features.Profile.Shared;

namespace Identity.Application.Features.Profile.Commands.ChangeOwnName;

public sealed class ChangeOwnNameValidator : AbstractValidator<ChangeOwnNameCommand>
{
    public ChangeOwnNameValidator()
    {
        RuleFor(x => x.FullName).FullName();
    }
}
