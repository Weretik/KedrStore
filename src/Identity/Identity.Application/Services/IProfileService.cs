using Identity.Application.Features.Avatars;
using Identity.Application.Features.Profile.Shared;

namespace Identity.Application.Services;

/// <summary>
/// The signed-in staff account's own profile. Email and password changes are for administrators only.
/// </summary>
public interface IProfileService
{
    Task<Result<ProfileDto>> GetAsync(CancellationToken cancellationToken = default);
    Task<Result> ChangeNameAsync(string fullName, CancellationToken cancellationToken = default);
    Task<Result<AvatarResult>> UploadAvatarAsync(AvatarUpload upload, CancellationToken cancellationToken = default);
    Task<Result> RemoveAvatarAsync(CancellationToken cancellationToken = default);
    Task<Result> ChangeEmailAsync(string email, string currentPassword, CancellationToken cancellationToken = default);
    Task<Result> ChangePasswordAsync(string currentPassword, string newPassword, CancellationToken cancellationToken = default);
}
