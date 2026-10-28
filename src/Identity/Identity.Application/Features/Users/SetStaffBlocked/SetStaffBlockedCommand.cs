namespace Identity.Application.Features.Users.SetStaffBlocked;

public sealed record SetStaffBlockedCommand(Guid UserId, bool Blocked) : ICommand<Result>;
