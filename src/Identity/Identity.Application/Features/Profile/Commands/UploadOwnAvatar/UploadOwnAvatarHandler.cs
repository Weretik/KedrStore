using Identity.Application.Features.Avatars;
using Identity.Application.Abstractions.Accounts;
using Identity.Application.Abstractions.Sessions;

namespace Identity.Application.Features.Profile.Commands.UploadOwnAvatar;

public sealed class UploadOwnAvatarHandler(IProfileService profileService)
    : ICommandHandler<UploadOwnAvatarCommand, Result<AvatarResult>>
{
    public async ValueTask<Result<AvatarResult>> Handle(UploadOwnAvatarCommand command, CancellationToken cancellationToken)
        => await profileService.UploadAvatarAsync(command.Upload, cancellationToken);
}
