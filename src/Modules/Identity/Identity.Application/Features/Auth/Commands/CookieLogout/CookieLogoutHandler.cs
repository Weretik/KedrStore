using Identity.Application.Abstractions.Accounts;
using Identity.Application.Abstractions.Sessions;

namespace Identity.Application.Features.Auth.Commands.CookieLogout;

public sealed class CookieLogoutHandler(ICookieSessionService cookieSessionService)
    : ICommandHandler<CookieLogoutCommand, Result>
{
    public async ValueTask<Result> Handle(CookieLogoutCommand command, CancellationToken cancellationToken)
    {
        await cookieSessionService.SignOutAsync(cancellationToken);
        return Result.Success();
    }
}
