using Identity.Application.Services;

namespace Identity.Application.Features.Auth.SessionRefresh;

public sealed class SessionRefreshCommandHandler(IIdentitySessionService identitySessionService)
    : ICommandHandler<SessionRefreshCommand, Result<SessionRefresh.DTOs.SessionRefreshResult>>
{
    public async ValueTask<Result<SessionRefresh.DTOs.SessionRefreshResult>> Handle(
        SessionRefreshCommand command,
        CancellationToken cancellationToken)
    {
        var result = await identitySessionService.RefreshAsync(command.Request.RefreshToken, cancellationToken);
        return result is null ? Result.Unauthorized() : Result.Success(result);
    }
}
