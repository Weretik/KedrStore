using Identity.Application.Features.Auth.Commands.SessionLogin;
using Identity.Application.Abstractions.Accounts;
using Identity.Application.Abstractions.Sessions;

namespace Identity.Application.Features.Auth.Commands.SessionLogin;

public sealed class SessionLoginHandler(IIdentitySessionService identitySessionService)
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
