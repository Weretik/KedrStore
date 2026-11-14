using FluentValidation;
using Identity.Application.Features.Auth.Shared;

namespace Identity.Application.Features.Auth.Commands.SessionLogin;

public sealed class SessionLoginValidator : AbstractValidator<SessionLoginCommand>
{
    public SessionLoginValidator()
    {
        RuleFor(x => x.Request).NotNull();
        When(x => x.Request is not null, () =>
        {
            RuleFor(x => x.Request.Email).NotEmpty().MaximumLength(LoginInputRules.MaxEmailLength);
            RuleFor(x => x.Request.Password).NotEmpty().MaximumLength(LoginInputRules.MaxPasswordLength);
        });
    }
}
