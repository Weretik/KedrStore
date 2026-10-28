using Identity.Application.Features.Users.Shared;

namespace Identity.Application.Features.Users.GetStaffList;

public sealed record GetStaffListQuery : IQuery<Result<IReadOnlyList<StaffAccountDto>>>;
