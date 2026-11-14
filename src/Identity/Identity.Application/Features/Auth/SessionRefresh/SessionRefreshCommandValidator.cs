using FluentValidation;
using Identity.Application.Features.Auth.Shared;

namespace Identity.Application.Features.Auth.SessionRefresh;

public sealed class SessionRefreshCommandValidator : AbstractValidator<SessionRefreshCommand>
{
    public SessionRefreshCommandValidator()
    {
        RuleFor(x => x.Request).NotNull();
        RuleFor(x => x.Request.RefreshToken).NotEmpty().MaximumLength(LoginInputRules.MaxRefreshTokenLength)
            .When(x => x.Request is not null);
    }
}
