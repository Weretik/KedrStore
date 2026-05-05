using Identity.Application.Features.Auth.SessionMe.DTOs;
using Identity.Application.Services;

namespace Identity.Application.Features.Auth.SessionMe;

public sealed class SessionMeQueryHandler(IIdentitySessionService identitySessionService)
    : IQueryHandler<SessionMeQuery, Result<SessionMeResult>>
{
    public async ValueTask<Result<SessionMeResult>> Handle(SessionMeQuery query, CancellationToken cancellationToken)
    {
        var result = await identitySessionService.GetCurrentUserAsync(cancellationToken);
        return result is null ? Result.Unauthorized() : Result.Success(result);
    }
}
