using Identity.Application.Features.Auth.Shared;
using Identity.Application.Features.Auth.Commands.SessionLogin;
using Identity.Application.Features.Auth.Queries.SessionMe;
using Identity.Application.Features.Auth.Commands.SessionRefresh;

namespace Identity.Application.Abstractions.Sessions;

public interface IIdentitySessionService
{
    Task<SessionLoginResult?> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<SessionRefreshResult?> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<SessionMeResult?> GetCurrentUserAsync(CancellationToken cancellationToken = default);
    Task<bool> LogoutAsync(string? refreshToken, CancellationToken cancellationToken = default);
}
