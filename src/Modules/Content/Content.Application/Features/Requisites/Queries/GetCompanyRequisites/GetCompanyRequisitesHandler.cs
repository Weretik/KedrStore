using Content.Application.Features.Requisites.Shared;

namespace Content.Application.Features.Requisites.Queries.GetCompanyRequisites;

public sealed class GetCompanyRequisitesHandler(IReadContentDbContext db)
    : IQueryHandler<GetCompanyRequisitesQuery, Result<CompanyRequisitesDetails>>
{
    public async ValueTask<Result<CompanyRequisitesDetails>> Handle(GetCompanyRequisitesQuery query,
        CancellationToken cancellationToken)
    {
        var requisites = await db.CompanyRequisites.AsNoTracking()
            .FirstOrDefaultAsync(row => row.Id == CompanyRequisites.SingletonId, cancellationToken);
        return requisites is null
            ? Result.NotFound()
            : Result.Success(CompanyRequisitesMapper.ToDetails(requisites));
    }
}
