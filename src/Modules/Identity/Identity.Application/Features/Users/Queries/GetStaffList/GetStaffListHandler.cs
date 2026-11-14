using Identity.Application.Features.Users.Shared;
using Identity.Application.Abstractions.Accounts;
using Identity.Application.Abstractions.Sessions;

namespace Identity.Application.Features.Users.Queries.GetStaffList;

public sealed class GetStaffListHandler(IStaffAccountService staffAccountService)
    : IQueryHandler<GetStaffListQuery, Result<IReadOnlyList<StaffAccountDto>>>
{
    public async ValueTask<Result<IReadOnlyList<StaffAccountDto>>> Handle(GetStaffListQuery query, CancellationToken cancellationToken)
        => Result.Success(await staffAccountService.GetListAsync(cancellationToken));
}
