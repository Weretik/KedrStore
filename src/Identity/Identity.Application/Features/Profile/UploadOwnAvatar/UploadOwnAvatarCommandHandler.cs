using Identity.Application.Features.Avatars;
using Identity.Application.Services;

namespace Identity.Application.Features.Profile.UploadOwnAvatar;

public sealed class UploadOwnAvatarCommandHandler(IProfileService profileService)
    : ICommandHandler<UploadOwnAvatarCommand, Result<AvatarResult>>
{
    public async ValueTask<Result<AvatarResult>> Handle(UploadOwnAvatarCommand command, CancellationToken cancellationToken)
        => await profileService.UploadAvatarAsync(command.Upload, cancellationToken);
}
