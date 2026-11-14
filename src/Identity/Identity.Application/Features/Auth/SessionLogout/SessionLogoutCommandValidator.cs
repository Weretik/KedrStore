using FluentValidation;
using Identity.Application.Features.Auth.Shared;

namespace Identity.Application.Features.Auth.SessionLogout;

public sealed class SessionLogoutCommandValidator : AbstractValidator<SessionLogoutCommand>
{
    public SessionLogoutCommandValidator()
        => RuleFor(x => x.RefreshToken).MaximumLength(LoginInputRules.MaxRefreshTokenLength);
}
