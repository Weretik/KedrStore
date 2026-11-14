using Identity.Application.Features.Avatars;

namespace Identity.Application.Features.Profile.Commands.UploadOwnAvatar;

public sealed record UploadOwnAvatarCommand(AvatarUpload Upload) : ICommand<Result<AvatarResult>>;
