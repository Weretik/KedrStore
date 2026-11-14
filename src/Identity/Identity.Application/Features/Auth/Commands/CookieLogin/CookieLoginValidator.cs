using FluentValidation;
using Identity.Application.Features.Auth.Shared;

namespace Identity.Application.Features.Auth.Commands.CookieLogin;

public sealed class CookieLoginValidator : AbstractValidator<CookieLoginCommand>
{
    public CookieLoginValidator()
    {
        RuleFor(x => x.Email).NotEmpty().MaximumLength(LoginInputRules.MaxEmailLength);
        RuleFor(x => x.Password).NotEmpty().MaximumLength(LoginInputRules.MaxPasswordLength);
    }
}
