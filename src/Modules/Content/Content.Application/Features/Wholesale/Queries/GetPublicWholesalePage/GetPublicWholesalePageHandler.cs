using Content.Application.Features.Wholesale.Shared;

namespace Content.Application.Features.Wholesale.Queries.GetPublicWholesalePage;

public sealed class GetPublicWholesalePageHandler(IReadContentDbContext db)
    : IQueryHandler<GetPublicWholesalePageQuery, Result<PublicWholesalePage>>
{
    public async ValueTask<Result<PublicWholesalePage>> Handle(GetPublicWholesalePageQuery query,
        CancellationToken cancellationToken)
    {
        var page = await db.WholesalePages.AsNoTracking()
            .FirstOrDefaultAsync(row => row.Id == WholesalePage.SingletonId, cancellationToken);
        return page is null
            ? Result.NotFound()
            : Result.Success(new PublicWholesalePage(
                WholesaleMapper.ToDto(query.Language, page.Text(query.Language)), page.UpdatedAt));
    }
}
