namespace Identity.Application.Features.Users.RemoveStaffAvatar;

public sealed record RemoveStaffAvatarCommand(Guid UserId) : ICommand<Result>;
