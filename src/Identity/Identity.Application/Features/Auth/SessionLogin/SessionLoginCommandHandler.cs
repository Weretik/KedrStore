using Identity.Application.Features.Auth.SessionLogin.DTOs;
using Identity.Application.Services;

namespace Identity.Application.Features.Auth.SessionLogin;

public sealed class SessionLoginCommandHandler(IIdentitySessionService identitySessionService)
    : ICommandHandler<SessionLoginCommand, Result<SessionLoginResult>>
{
    public async ValueTask<Result<SessionLoginResult>> Handle(SessionLoginCommand command, CancellationToken cancellationToken)
    {
        var result = await identitySessionService.LoginAsync(
            command.Request.Email,
            command.Request.Password,
            cancellationToken);

        return result is null ? Result.Unauthorized() : Result.Success(result);
    }
}
