using Content.Application.Features.SiteSeo.Shared;

namespace Content.Application.Features.SiteSeo.Queries.ListPublicPageSeo;

public sealed class ListPublicPageSeoHandler(IReadContentDbContext db)
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
