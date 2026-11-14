using Identity.Application.Abstractions.Accounts;
using Identity.Application.Abstractions.Sessions;

namespace Identity.Application.Features.Users.Commands.RemoveStaffAvatar;

public sealed class RemoveStaffAvatarHandler(IStaffProfileEditService staffProfileEditService)
    : ICommandHandler<RemoveStaffAvatarCommand, Result>
{
    public async ValueTask<Result> Handle(RemoveStaffAvatarCommand command, CancellationToken cancellationToken)
        => await staffProfileEditService.RemoveAvatarAsync(command.UserId, cancellationToken);
}
