using FluentValidation;
using Identity.Application.Features.Auth.Shared;

namespace Identity.Application.Features.Auth.Commands.SessionRefresh;

public sealed class SessionRefreshValidator : AbstractValidator<SessionRefreshCommand>
{
    public SessionRefreshValidator()
    {
        RuleFor(x => x.Request).NotNull();
        RuleFor(x => x.Request.RefreshToken).NotEmpty().MaximumLength(LoginInputRules.MaxRefreshTokenLength)
            .When(x => x.Request is not null);
    }
}
