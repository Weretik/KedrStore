using Content.Application.Features.Wholesale.Shared;

namespace Content.Application.Features.Wholesale.Queries.GetAdminWholesalePage;

public sealed class GetAdminWholesalePageHandler(IReadContentDbContext db)
    : IQueryHandler<GetAdminWholesalePageQuery, Result<AdminWholesalePage>>
{
    public async ValueTask<Result<AdminWholesalePage>> Handle(GetAdminWholesalePageQuery query,
        CancellationToken cancellationToken)
    {
        var page = await db.WholesalePages.AsNoTracking()
            .FirstOrDefaultAsync(row => row.Id == WholesalePage.SingletonId, cancellationToken);
        return page is null ? Result.NotFound() : Result.Success(WholesaleMapper.ToAdmin(page));
    }
}
