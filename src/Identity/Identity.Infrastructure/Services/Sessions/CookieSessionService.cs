using Identity.Application.Features.Auth.SessionMe.DTOs;
using Identity.Application.Services;
using Identity.Infrastructure.Entities;

namespace Identity.Infrastructure.Services.Sessions;

public sealed class CookieSessionService(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    ILogger<CookieSessionService> logger) : ICookieSessionService
{
    public async Task<SessionMeResult?> SignInAsync(string email, string password, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("AUDIT: Cookie login failed: empty credentials");
            return null;
        }

        var user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            logger.LogWarning("AUDIT: Cookie login failed: user not found for email {Email}", email);
            return null;
        }

        var result = await signInManager.PasswordSignInAsync(user, password, isPersistent: true, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            logger.LogWarning("AUDIT: Cookie login failed for userId {UserId}; locked out: {IsLockedOut}", user.Id, result.IsLockedOut);
            return null;
        }

        var roles = await userManager.GetRolesAsync(user);
        logger.LogInformation("AUDIT: Cookie login success for userId {UserId}", user.Id);
        return new SessionMeResult(user.Id.ToString(), user.Email ?? string.Empty, user.FullName, roles.ToArray());
    }

    public async Task SignOutAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await signInManager.SignOutAsync();
    }
}
