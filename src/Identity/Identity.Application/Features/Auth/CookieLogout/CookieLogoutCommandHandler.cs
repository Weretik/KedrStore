using Identity.Application.Services;

namespace Identity.Application.Features.Auth.CookieLogout;

public sealed class CookieLogoutCommandHandler(ICookieSessionService cookieSessionService)
    : ICommandHandler<CookieLogoutCommand, Result>
{
    public async ValueTask<Result> Handle(CookieLogoutCommand command, CancellationToken cancellationToken)
    {
        await cookieSessionService.SignOutAsync(cancellationToken);
        return Result.Success();
    }
}
