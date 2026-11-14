using Identity.Application.Features.Users.Shared;

namespace Identity.Application.Features.Users.Queries.GetStaffList;

public sealed record GetStaffListQuery : IQuery<Result<IReadOnlyList<StaffAccountDto>>>;
