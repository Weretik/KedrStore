using Identity.Application.Features.Users.Shared;
using Identity.Application.Services;

namespace Identity.Application.Features.Users.SearchStaffCandidates;

public sealed class SearchStaffCandidatesQueryHandler(IStaffAccountService staffAccountService)
    : IQueryHandler<SearchStaffCandidatesQuery, Result<IReadOnlyList<StaffCandidateDto>>>
{
    public async ValueTask<Result<IReadOnlyList<StaffCandidateDto>>> Handle(
        SearchStaffCandidatesQuery query,
        CancellationToken cancellationToken)
        => Result.Success(await staffAccountService.SearchCandidatesAsync(query.Search.Trim(), cancellationToken));
}
