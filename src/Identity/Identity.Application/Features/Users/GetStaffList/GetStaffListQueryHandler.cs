using Identity.Application.Features.Users.Shared;
using Identity.Application.Services;

namespace Identity.Application.Features.Users.GetStaffList;

public sealed class GetStaffListQueryHandler(IStaffAccountService staffAccountService)
    : IQueryHandler<GetStaffListQuery, Result<IReadOnlyList<StaffAccountDto>>>
{
    public async ValueTask<Result<IReadOnlyList<StaffAccountDto>>> Handle(GetStaffListQuery query, CancellationToken cancellationToken)
        => Result.Success(await staffAccountService.GetListAsync(cancellationToken));
}
