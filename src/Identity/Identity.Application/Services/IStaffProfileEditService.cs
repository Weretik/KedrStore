using Identity.Application.Features.Avatars;

namespace Identity.Application.Services;

/// <summary>
/// An administrator edits a manager's profile. Administrator accounts cannot be edited here (403).
/// </summary>
public interface IStaffProfileEditService
{
    Task<Result> ChangeNameAsync(Guid userId, string fullName, CancellationToken cancellationToken = default);
    Task<Result> ChangeEmailAsync(Guid userId, string email, CancellationToken cancellationToken = default);
    Task<Result<AvatarResult>> UploadAvatarAsync(Guid userId, AvatarUpload upload, CancellationToken cancellationToken = default);
    Task<Result> RemoveAvatarAsync(Guid userId, CancellationToken cancellationToken = default);
}
