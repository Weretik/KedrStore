using Identity.Application.Features.Auth.Queries.SessionMe;
using Identity.Application.Abstractions.Accounts;
using Identity.Application.Abstractions.Sessions;

namespace Identity.Application.Features.Auth.Queries.SessionMe;

public sealed class SessionMeHandler(IIdentitySessionService identitySessionService)
    : IQueryHandler<SessionMeQuery, Result<SessionMeResult>>
{
    public async ValueTask<Result<SessionMeResult>> Handle(SessionMeQuery query, CancellationToken cancellationToken)
    {
        var result = await identitySessionService.GetCurrentUserAsync(cancellationToken);
        return result is null ? Result.Unauthorized() : Result.Success(result);
    }
}
