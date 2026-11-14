using Identity.Application.Features.Avatars;
using Identity.Application.Abstractions.Accounts;
using Identity.Application.Abstractions.Sessions;

namespace Identity.Application.Features.Users.Commands.UploadStaffAvatar;

public sealed class UploadStaffAvatarHandler(IStaffProfileEditService staffProfileEditService)
    : ICommandHandler<UploadStaffAvatarCommand, Result<AvatarResult>>
{
    public async ValueTask<Result<AvatarResult>> Handle(UploadStaffAvatarCommand command, CancellationToken cancellationToken)
        => await staffProfileEditService.UploadAvatarAsync(command.UserId, command.Upload, cancellationToken);
}
