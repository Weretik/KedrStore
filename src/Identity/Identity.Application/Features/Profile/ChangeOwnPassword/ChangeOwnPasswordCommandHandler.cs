using Identity.Application.Services;

namespace Identity.Application.Features.Profile.ChangeOwnPassword;

public sealed class ChangeOwnPasswordCommandHandler(IProfileService profileService)
    : ICommandHandler<ChangeOwnPasswordCommand, Result>
{
    public async ValueTask<Result> Handle(ChangeOwnPasswordCommand command, CancellationToken cancellationToken)
        => await profileService.ChangePasswordAsync(command.CurrentPassword, command.NewPassword, cancellationToken);
}
