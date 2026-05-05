using Identity.Application.Features.Auth.SessionLogin.DTOs;
using Identity.Application.Features.Auth.SessionMe.DTOs;
using Identity.Application.Features.Auth.SessionRefresh.DTOs;

namespace Identity.Application.Services;

public interface IIdentitySessionService
{
    Task<SessionLoginResult?> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<SessionRefreshResult?> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<SessionMeResult?> GetCurrentUserAsync(CancellationToken cancellationToken = default);
    Task<bool> LogoutAsync(CancellationToken cancellationToken = default);
}
