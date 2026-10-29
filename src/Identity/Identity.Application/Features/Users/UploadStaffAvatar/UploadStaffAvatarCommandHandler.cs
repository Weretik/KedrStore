using Identity.Application.Features.Avatars;
using Identity.Application.Services;

namespace Identity.Application.Features.Users.UploadStaffAvatar;

public sealed class UploadStaffAvatarCommandHandler(IStaffProfileEditService staffProfileEditService)
    : ICommandHandler<UploadStaffAvatarCommand, Result<AvatarResult>>
{
    public async ValueTask<Result<AvatarResult>> Handle(UploadStaffAvatarCommand command, CancellationToken cancellationToken)
        => await staffProfileEditService.UploadAvatarAsync(command.UserId, command.Upload, cancellationToken);
}
