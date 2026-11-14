using Identity.Application.Features.Users.Shared;
using Identity.Application.Abstractions.Accounts;
using Identity.Application.Abstractions.Sessions;

namespace Identity.Application.Features.Users.Queries.SearchStaffCandidates;

public sealed class SearchStaffCandidatesHandler(IStaffAccountService staffAccountService)
    : IQueryHandler<SearchStaffCandidatesQuery, Result<IReadOnlyList<StaffCandidateDto>>>
{
    public async ValueTask<Result<IReadOnlyList<StaffCandidateDto>>> Handle(
        SearchStaffCandidatesQuery query,
        CancellationToken cancellationToken)
        => Result.Success(await staffAccountService.SearchCandidatesAsync(query.Search.Trim(), cancellationToken));
}
