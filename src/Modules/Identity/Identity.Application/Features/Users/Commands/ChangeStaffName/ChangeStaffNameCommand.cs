namespace Identity.Application.Features.Users.Commands.ChangeStaffName;

public sealed record ChangeStaffNameCommand(Guid UserId, string FullName) : ICommand<Result>;
