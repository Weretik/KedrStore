using Identity.Application.Abstractions.Accounts;
using Identity.Application.Abstractions.Sessions;

namespace Identity.Application.Features.Profile.Commands.ChangeOwnName;

public sealed class ChangeOwnNameHandler(IProfileService profileService)
    : ICommandHandler<ChangeOwnNameCommand, Result>
{
    public async ValueTask<Result> Handle(ChangeOwnNameCommand command, CancellationToken cancellationToken)
        => await profileService.ChangeNameAsync(command.FullName, cancellationToken);
}
