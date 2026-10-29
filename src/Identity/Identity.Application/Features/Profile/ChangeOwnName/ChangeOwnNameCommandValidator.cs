using FluentValidation;
using Identity.Application.Features.Profile.Shared;

namespace Identity.Application.Features.Profile.ChangeOwnName;

public sealed class ChangeOwnNameCommandValidator : AbstractValidator<ChangeOwnNameCommand>
{
    public ChangeOwnNameCommandValidator()
    {
        RuleFor(x => x.FullName).FullName();
    }
}
