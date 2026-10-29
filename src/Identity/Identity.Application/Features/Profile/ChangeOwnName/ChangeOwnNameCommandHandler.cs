using Identity.Application.Services;

namespace Identity.Application.Features.Profile.ChangeOwnName;

public sealed class ChangeOwnNameCommandHandler(IProfileService profileService)
    : ICommandHandler<ChangeOwnNameCommand, Result>
{
    public async ValueTask<Result> Handle(ChangeOwnNameCommand command, CancellationToken cancellationToken)
        => await profileService.ChangeNameAsync(command.FullName, cancellationToken);
}
