using Identity.Application.Features.Auth.SessionMe.DTOs;

namespace Identity.Application.Services;

/// <summary>
/// Browser session: signs in with the Identity cookie, so no token reaches JavaScript.
/// </summary>
public interface ICookieSessionService
{
    Task<SessionMeResult?> SignInAsync(string email, string password, CancellationToken cancellationToken = default);
    Task SignOutAsync(CancellationToken cancellationToken = default);
}
