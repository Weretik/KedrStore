using Ardalis.Result;
using Identity.Application.Features.Avatars;
using Identity.Application.Features.Profile.Shared;
using Identity.Application.Services;
using Identity.Domain.Authorization;
using Identity.Infrastructure.Avatars;
using Identity.Infrastructure.Entities;
using Identity.Infrastructure.Services.Sessions;
using Identity.Infrastructure.Services.Users;

namespace Identity.Infrastructure.Services.Profile;

/// <summary>
/// The signed-in staff account's own profile. Controller policies decide who may call what;
/// email and password changes additionally require the current password.
/// </summary>
public sealed class ProfileService(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    AccountEmailWriter emailWriter,
    AvatarService avatarService,
    AvatarUrlBuilder avatarUrlBuilder,
    RefreshSessionStore refreshSessionStore,
    IHttpContextAccessor httpContextAccessor,
    ILogger<ProfileService> logger) : IProfileService
{
    public const string WrongPasswordMessage = "The current password is incorrect.";

    public async Task<Result<ProfileDto>> GetAsync(CancellationToken cancellationToken = default)
    {
        var user = await CurrentUserAsync();
        if (user is null)
        {
            return Result<ProfileDto>.Unauthorized();
        }

        var roles = await userManager.GetRolesAsync(user);
        var role = roles.Contains(RoleNames.Admin, StringComparer.Ordinal) ? RoleNames.Admin : RoleNames.Manager;
        return Result.Success(new ProfileDto(user.Id, user.Email ?? string.Empty, user.FullName, role, avatarUrlBuilder.Build(user.AvatarPath)));
    }

    public async Task<Result> ChangeNameAsync(string fullName, CancellationToken cancellationToken = default)
    {
        var user = await CurrentUserAsync();
        if (user is null)
        {
            return Result.Unauthorized();
        }

        user.FullName = fullName.Trim();
        var updated = await userManager.UpdateAsync(user);
        return updated.Succeeded ? Result.Success() : Invalid(updated);
    }

    public async Task<Result<AvatarResult>> UploadAvatarAsync(AvatarUpload upload, CancellationToken cancellationToken = default)
    {
        var user = await CurrentUserAsync();
        return user is null
            ? Result<AvatarResult>.Unauthorized()
            : await avatarService.ReplaceAsync(user, upload, cancellationToken);
    }

    public async Task<Result> RemoveAvatarAsync(CancellationToken cancellationToken = default)
    {
        var user = await CurrentUserAsync();
        return user is null ? Result.Unauthorized() : await avatarService.RemoveAsync(user, cancellationToken);
    }

    public async Task<Result> ChangeEmailAsync(string email, string currentPassword, CancellationToken cancellationToken = default)
    {
        var user = await CurrentUserAsync();
        if (user is null)
        {
            return Result.Unauthorized();
        }

        if (!await IsCurrentPasswordAsync(user, currentPassword))
        {
            return WrongPassword();
        }

        var changed = await emailWriter.ChangeAsync(user, email);
        if (!changed.IsSuccess)
        {
            return changed;
        }

        await KeepOnlyCurrentSessionAsync(user, "EmailChanged", cancellationToken);
        logger.LogInformation("AUDIT: User {UserId} changed own email", user.Id);
        return Result.Success();
    }

    public async Task<Result> ChangePasswordAsync(string currentPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        var user = await CurrentUserAsync();
        if (user is null)
        {
            return Result.Unauthorized();
        }

        if (!await IsCurrentPasswordAsync(user, currentPassword))
        {
            return WrongPassword();
        }

        var changed = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (!changed.Succeeded)
        {
            return Invalid(changed);
        }

        await KeepOnlyCurrentSessionAsync(user, "PasswordChanged", cancellationToken);
        logger.LogInformation("AUDIT: User {UserId} changed own password", user.Id);
        return Result.Success();
    }

    // Counts a wrong password towards lockout, like a failed sign-in.
    private async Task<bool> IsCurrentPasswordAsync(AppUser user, string password) =>
        (await signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true)).Succeeded;

    /// <summary>
    /// The security stamp already changed, which ends other browser sessions. Refresh sessions are revoked,
    /// and a browser caller gets a fresh cookie so it stays signed in.
    /// </summary>
    private async Task KeepOnlyCurrentSessionAsync(AppUser user, string reason, CancellationToken cancellationToken)
    {
        await refreshSessionStore.RevokeAllActiveByUserAsync(user.Id, DateTimeOffset.UtcNow, reason, cancellationToken);
        if (!IsBearerRequest())
        {
            await signInManager.RefreshSignInAsync(user);
        }
    }

    private bool IsBearerRequest() =>
        httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString()
            .StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == true;

    private async Task<AppUser?> CurrentUserAsync()
    {
        var principal = httpContextAccessor.HttpContext?.User;
        return principal is null ? null : await userManager.GetUserAsync(principal);
    }

    private static Result WrongPassword() =>
        Result.Invalid(new ValidationError("currentPassword", WrongPasswordMessage, null, ValidationSeverity.Error));

    private static Result Invalid(IdentityResult result) =>
        Result.Invalid(result.Errors.Select(error => new ValidationError(error.Description)).ToList());
}
