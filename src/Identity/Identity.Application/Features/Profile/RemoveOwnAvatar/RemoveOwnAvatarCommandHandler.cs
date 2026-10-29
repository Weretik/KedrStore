using Identity.Application.Services;

namespace Identity.Application.Features.Profile.RemoveOwnAvatar;

public sealed class RemoveOwnAvatarCommandHandler(IProfileService profileService)
    : ICommandHandler<RemoveOwnAvatarCommand, Result>
{
    public async ValueTask<Result> Handle(RemoveOwnAvatarCommand command, CancellationToken cancellationToken)
        => await profileService.RemoveAvatarAsync(cancellationToken);
}
