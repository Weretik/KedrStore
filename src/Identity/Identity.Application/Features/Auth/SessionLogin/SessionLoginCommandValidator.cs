using FluentValidation;
using Identity.Application.Features.Auth.Shared;

namespace Identity.Application.Features.Auth.SessionLogin;

public sealed class SessionLoginCommandValidator : AbstractValidator<SessionLoginCommand>
{
    public SessionLoginCommandValidator()
    {
        RuleFor(x => x.Request).NotNull();
        When(x => x.Request is not null, () =>
        {
            RuleFor(x => x.Request.Email).NotEmpty().MaximumLength(LoginInputRules.MaxEmailLength);
            RuleFor(x => x.Request.Password).NotEmpty().MaximumLength(LoginInputRules.MaxPasswordLength);
        });
    }
}
