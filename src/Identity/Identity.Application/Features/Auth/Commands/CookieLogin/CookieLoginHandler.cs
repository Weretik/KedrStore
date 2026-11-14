using Identity.Application.Features.Auth.Shared;
using Identity.Application.Abstractions.Accounts;
using Identity.Application.Abstractions.Sessions;

namespace Identity.Application.Features.Auth.Commands.CookieLogin;

public sealed class CookieLoginHandler(ICookieSessionService cookieSessionService)
    : ICommandHandler<CookieLoginCommand, Result<SessionMeResult>>
{
    public async ValueTask<Result<SessionMeResult>> Handle(CookieLoginCommand command, CancellationToken cancellationToken)
    {
        var result = await cookieSessionService.SignInAsync(command.Email, command.Password, cancellationToken);
        return result is null ? Result.Unauthorized() : Result.Success(result);
    }
}
