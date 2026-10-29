using Ardalis.Result;
using Identity.Application.Features.Avatars;
using Identity.Infrastructure.Entities;

namespace Identity.Infrastructure.Avatars;

/// <summary>
/// Replaces or removes a user's avatar: process, upload under a new name, point the user at it,
/// then delete the previous file. Shared by the own profile and the Users page.
/// </summary>
public sealed class AvatarService(
    AvatarImageProcessor processor,
    IAvatarStorage storage,
    AvatarUrlBuilder urlBuilder,
    UserManager<AppUser> userManager,
    ILogger<AvatarService> logger)
{
    public const string StorageUnavailableMessage = "Avatar storage is not configured.";

    public async Task<Result<AvatarResult>> ReplaceAsync(AppUser user, AvatarUpload upload, CancellationToken cancellationToken = default)
    {
        if (!storage.IsConfigured)
        {
            return Result<AvatarResult>.Unavailable(StorageUnavailableMessage);
        }

        var processed = await processor.ProcessAsync(upload, cancellationToken);
        if (!processed.IsSuccess)
        {
            return Result<AvatarResult>.Invalid(processed.ValidationErrors);
        }

        // A new name per upload, so CDN and browser caches never show the previous picture.
        var key = $"{user.Id:N}/{Guid.NewGuid():N}.webp";
        await storage.SaveAsync(key, processed.Value, cancellationToken);

        var previousKey = user.AvatarPath;
        user.AvatarPath = key;
        var updated = await userManager.UpdateAsync(user);
        if (!updated.Succeeded)
        {
            user.AvatarPath = previousKey;
            await DeleteQuietlyAsync(key, cancellationToken);
            return Result<AvatarResult>.Invalid(updated.Errors.Select(error => new ValidationError(error.Description)).ToList());
        }

        await DeleteQuietlyAsync(previousKey, cancellationToken);
        logger.LogInformation("AUDIT: Avatar of user {UserId} replaced", user.Id);
        return Result.Success(new AvatarResult(urlBuilder.Build(key)!));
    }

    public async Task<Result> RemoveAsync(AppUser user, CancellationToken cancellationToken = default)
    {
        var previousKey = user.AvatarPath;
        if (previousKey is null)
        {
            return Result.Success();
        }

        user.AvatarPath = null;
        var updated = await userManager.UpdateAsync(user);
        if (!updated.Succeeded)
        {
            return Result.Invalid(updated.Errors.Select(error => new ValidationError(error.Description)).ToList());
        }

        await DeleteQuietlyAsync(previousKey, cancellationToken);
        logger.LogInformation("AUDIT: Avatar of user {UserId} removed", user.Id);
        return Result.Success();
    }

    private async Task DeleteQuietlyAsync(string? key, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(key) || !storage.IsConfigured)
        {
            return;
        }

        try
        {
            await storage.DeleteAsync(key, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            // The database no longer points at the file; a leftover file is harmless.
            logger.LogWarning(exception, "Could not delete avatar file {AvatarKey}", key);
        }
    }
}
