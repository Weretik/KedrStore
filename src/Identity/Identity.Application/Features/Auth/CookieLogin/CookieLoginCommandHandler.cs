using Identity.Application.Features.Auth.SessionMe.DTOs;
using Identity.Application.Services;

namespace Identity.Application.Features.Auth.CookieLogin;

public sealed class CookieLoginCommandHandler(ICookieSessionService cookieSessionService)
    : ICommandHandler<CookieLoginCommand, Result<SessionMeResult>>
{
    public async ValueTask<Result<SessionMeResult>> Handle(CookieLoginCommand command, CancellationToken cancellationToken)
    {
        var result = await cookieSessionService.SignInAsync(command.Email, command.Password, cancellationToken);
        return result is null ? Result.Unauthorized() : Result.Success(result);
    }
}
