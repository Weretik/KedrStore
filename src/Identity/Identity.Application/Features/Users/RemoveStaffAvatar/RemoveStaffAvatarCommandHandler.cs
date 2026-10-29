using Identity.Application.Services;

namespace Identity.Application.Features.Users.RemoveStaffAvatar;

public sealed class RemoveStaffAvatarCommandHandler(IStaffProfileEditService staffProfileEditService)
    : ICommandHandler<RemoveStaffAvatarCommand, Result>
{
    public async ValueTask<Result> Handle(RemoveStaffAvatarCommand command, CancellationToken cancellationToken)
        => await staffProfileEditService.RemoveAvatarAsync(command.UserId, cancellationToken);
}
