namespace Identity.Application.Features.Users.Commands.SetStaffBlocked;

public sealed record SetStaffBlockedCommand(Guid UserId, bool Blocked) : ICommand<Result>;
