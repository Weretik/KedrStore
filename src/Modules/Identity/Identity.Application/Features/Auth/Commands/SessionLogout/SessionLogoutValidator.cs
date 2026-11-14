using FluentValidation;
using Identity.Application.Features.Auth.Shared;

namespace Identity.Application.Features.Auth.Commands.SessionLogout;

public sealed class SessionLogoutValidator : AbstractValidator<SessionLogoutCommand>
{
    public SessionLogoutValidator()
        => RuleFor(x => x.RefreshToken).MaximumLength(LoginInputRules.MaxRefreshTokenLength);
}
