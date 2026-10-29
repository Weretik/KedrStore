using Identity.Application.Features.Avatars;

namespace Identity.Application.Features.Profile.UploadOwnAvatar;

public sealed record UploadOwnAvatarCommand(AvatarUpload Upload) : ICommand<Result<AvatarResult>>;
