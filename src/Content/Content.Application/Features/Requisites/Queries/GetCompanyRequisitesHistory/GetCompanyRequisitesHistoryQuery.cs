using Content.Application.Features.Requisites.Shared;

namespace Content.Application.Features.Requisites.Queries.GetCompanyRequisitesHistory;

/// <summary>The latest saved changes of the requisites, newest first.</summary>
public sealed record GetCompanyRequisitesHistoryQuery(int Limit = GetCompanyRequisitesHistoryQuery.DefaultLimit)
    : IQuery<Result<IReadOnlyList<CompanyRequisitesChangeDetails>>>
{
    public const int DefaultLimit = 50;
}
