using Identity.Application.Features.Avatars;

namespace Identity.Application.Features.Users.UploadStaffAvatar;

public sealed record UploadStaffAvatarCommand(Guid UserId, AvatarUpload Upload) : ICommand<Result<AvatarResult>>;
