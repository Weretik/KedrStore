using Identity.Application.Services;

namespace Identity.Application.Features.Auth.SessionLogout;

public sealed class SessionLogoutCommandHandler(IIdentitySessionService identitySessionService)
    : ICommandHandler<SessionLogoutCommand, Result>
{
    public async ValueTask<Result> Handle(SessionLogoutCommand command, CancellationToken cancellationToken)
    {
        var success = await identitySessionService.LogoutAsync(cancellationToken);
        return success ? Result.Success() : Result.Unauthorized();
    }
}
