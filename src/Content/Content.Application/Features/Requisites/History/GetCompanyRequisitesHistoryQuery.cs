using Content.Application.Features.Requisites.Common;

namespace Content.Application.Features.Requisites.History;

/// <summary>The latest saved changes of the requisites, newest first.</summary>
public sealed record GetCompanyRequisitesHistoryQuery(int Limit = GetCompanyRequisitesHistoryQuery.DefaultLimit)
    : IQuery<Result<IReadOnlyList<CompanyRequisitesChangeDetails>>>
{
    public const int DefaultLimit = 50;
}
