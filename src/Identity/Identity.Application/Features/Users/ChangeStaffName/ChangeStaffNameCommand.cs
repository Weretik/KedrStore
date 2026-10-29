namespace Identity.Application.Features.Users.ChangeStaffName;

public sealed record ChangeStaffNameCommand(Guid UserId, string FullName) : ICommand<Result>;
