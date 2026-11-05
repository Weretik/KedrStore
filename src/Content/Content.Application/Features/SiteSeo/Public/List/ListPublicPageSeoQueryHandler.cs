using Content.Application.Features.SiteSeo.Common;

namespace Content.Application.Features.SiteSeo.Public.List;

public sealed class ListPublicPageSeoQueryHandler(IReadContentDbContext db)
    : IQueryHandler<ListPublicPageSeoQuery, Result<IReadOnlyList<PublicPageSeo>>>
{
    public async ValueTask<Result<IReadOnlyList<PublicPageSeo>>> Handle(ListPublicPageSeoQuery query,
        CancellationToken cancellationToken)
    {
        var rows = await db.PageSeo.AsNoTracking().ToListAsync(cancellationToken);
        IReadOnlyList<PublicPageSeo> pages = rows
            .Where(row => SitePages.IsKnown(row.Page))
            .Select(row => new PublicPageSeo(row.Page, row.Title(query.Language), row.Description(query.Language)))
            .Where(row => row.Title is not null || row.Description is not null)
            .OrderBy(row => SitePages.All.ToList().IndexOf(row.Page))
            .ToList();
        return Result.Success(pages);
    }
}
