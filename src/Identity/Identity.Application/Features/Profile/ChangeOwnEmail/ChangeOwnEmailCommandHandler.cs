using Identity.Application.Services;

namespace Identity.Application.Features.Profile.ChangeOwnEmail;

public sealed class ChangeOwnEmailCommandHandler(IProfileService profileService)
    : ICommandHandler<ChangeOwnEmailCommand, Result>
{
    public async ValueTask<Result> Handle(ChangeOwnEmailCommand command, CancellationToken cancellationToken)
        => await profileService.ChangeEmailAsync(command.Email, command.CurrentPassword, cancellationToken);
}
