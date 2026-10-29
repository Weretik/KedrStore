using Ardalis.Result;
using Identity.Application.Features.Avatars;
using Identity.Application.Services;
using Identity.Infrastructure.Avatars;
using Identity.Infrastructure.Entities;
using Identity.Infrastructure.Services.Sessions;

namespace Identity.Infrastructure.Services.Users;

public sealed class StaffProfileEditService(
    ManagedStaffResolver resolver,
    UserManager<AppUser> userManager,
    AccountEmailWriter emailWriter,
    AvatarService avatarService,
    RefreshSessionStore refreshSessionStore,
    IHttpContextAccessor httpContextAccessor,
    ILogger<StaffProfileEditService> logger) : IStaffProfileEditService
{
    public async Task<Result> ChangeNameAsync(Guid userId, string fullName, CancellationToken cancellationToken = default)
    {
        var target = await resolver.ResolveAsync(userId);
        if (target.User is null)
        {
            return target.Failure;
        }

        target.User.FullName = fullName.Trim();
        var updated = await userManager.UpdateAsync(target.User);
        if (!updated.Succeeded)
        {
            return Result.Invalid(updated.Errors.Select(error => new ValidationError(error.Description)).ToList());
        }

        logger.LogInformation("AUDIT: Staff account {UserId} name changed by {ActorId}", userId, ActorId());
        return Result.Success();
    }

    public async Task<Result> ChangeEmailAsync(Guid userId, string email, CancellationToken cancellationToken = default)
    {
        var target = await resolver.ResolveAsync(userId);
        if (target.User is null)
        {
            return target.Failure;
        }

        var changed = await emailWriter.ChangeAsync(target.User, email);
        if (!changed.IsSuccess)
        {
            return changed;
        }

        // The manager signs in again with the new email on every device.
        await refreshSessionStore.RevokeAllActiveByUserAsync(userId, DateTimeOffset.UtcNow, "EmailChanged", cancellationToken);
        logger.LogInformation("AUDIT: Staff account {UserId} email changed by {ActorId}", userId, ActorId());
        return Result.Success();
    }

    public async Task<Result<AvatarResult>> UploadAvatarAsync(Guid userId, AvatarUpload upload, CancellationToken cancellationToken = default)
    {
        var target = await resolver.ResolveAsync(userId);
        if (target.User is null)
        {
            return target.Failure.Status == ResultStatus.Forbidden
                ? Result<AvatarResult>.Forbidden()
                : Result<AvatarResult>.NotFound();
        }

        return await avatarService.ReplaceAsync(target.User, upload, cancellationToken);
    }

    public async Task<Result> RemoveAvatarAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var target = await resolver.ResolveAsync(userId);
        return target.User is null ? target.Failure : await avatarService.RemoveAsync(target.User, cancellationToken);
    }

    private string? ActorId() => httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
}
