using Identity.Application.Abstractions.Accounts;
using Identity.Application.Abstractions.Sessions;

namespace Identity.Application.Features.Auth.Commands.SessionRefresh;

public sealed class SessionRefreshHandler(IIdentitySessionService identitySessionService)
    : ICommandHandler<SessionRefreshCommand, Result<SessionRefreshResult>>
{
    public async ValueTask<Result<SessionRefreshResult>> Handle(
        SessionRefreshCommand command,
        CancellationToken cancellationToken)
    {
        var result = await identitySessionService.RefreshAsync(command.Request.RefreshToken, cancellationToken);
        return result is null ? Result.Unauthorized() : Result.Success(result);
    }
}
