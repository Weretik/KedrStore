using FluentValidation;
using Identity.Application.Features.Auth.Shared;

namespace Identity.Application.Features.Auth.CookieLogin;

public sealed class CookieLoginCommandValidator : AbstractValidator<CookieLoginCommand>
{
    public CookieLoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().MaximumLength(LoginInputRules.MaxEmailLength);
        RuleFor(x => x.Password).NotEmpty().MaximumLength(LoginInputRules.MaxPasswordLength);
    }
}
