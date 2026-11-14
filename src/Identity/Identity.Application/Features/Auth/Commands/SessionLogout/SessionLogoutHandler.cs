using Identity.Application.Abstractions.Accounts;
using Identity.Application.Abstractions.Sessions;

namespace Identity.Application.Features.Auth.Commands.SessionLogout;

public sealed class SessionLogoutHandler(IIdentitySessionService identitySessionService)
    : ICommandHandler<SessionLogoutCommand, Result>
{
    public async ValueTask<Result> Handle(SessionLogoutCommand command, CancellationToken cancellationToken)
    {
        var success = await identitySessionService.LogoutAsync(command.RefreshToken, cancellationToken);
        return success ? Result.Success() : Result.Unauthorized();
    }
}
