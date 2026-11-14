using Content.Application.Features.Requisites.Shared;

namespace Content.Application.Features.Requisites.Queries.GetCompanyRequisitesHistory;

public sealed class GetCompanyRequisitesHistoryHandler(IReadContentDbContext db)
    : IQueryHandler<GetCompanyRequisitesHistoryQuery, Result<IReadOnlyList<CompanyRequisitesChangeDetails>>>
{
    public async ValueTask<Result<IReadOnlyList<CompanyRequisitesChangeDetails>>> Handle(
        GetCompanyRequisitesHistoryQuery query, CancellationToken cancellationToken)
    {
        var limit = Math.Clamp(query.Limit, 1, GetCompanyRequisitesHistoryQuery.DefaultLimit);
        var entries = await db.CompanyRequisitesChanges.AsNoTracking()
            .OrderByDescending(entry => entry.ChangedAt)
            .ThenByDescending(entry => entry.Id)
            .Take(limit)
            .ToListAsync(cancellationToken);
        return Result.Success<IReadOnlyList<CompanyRequisitesChangeDetails>>(
            entries.Select(CompanyRequisitesMapper.ToDetails).ToList());
    }
}
