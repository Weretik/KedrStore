using Identity.Application.Features.Users.Shared;

namespace Identity.Application.Features.Users.SearchStaffCandidates;

/// <summary>Finds existing non-staff accounts by part of the email or name.</summary>
public sealed record SearchStaffCandidatesQuery(string Search) : IQuery<Result<IReadOnlyList<StaffCandidateDto>>>;
