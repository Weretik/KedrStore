namespace Identity.Application.Features.Users.Commands.RemoveStaffAvatar;

public sealed record RemoveStaffAvatarCommand(Guid UserId) : ICommand<Result>;
