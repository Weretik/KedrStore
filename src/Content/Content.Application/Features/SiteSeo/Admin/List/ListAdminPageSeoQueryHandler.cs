using Content.Application.Features.SiteSeo.Common;

namespace Content.Application.Features.SiteSeo.Admin.List;

public sealed class ListAdminPageSeoQueryHandler(IReadContentDbContext db)
    : IQueryHandler<ListAdminPageSeoQuery, Result<IReadOnlyList<AdminPageSeo>>>
{
    public async ValueTask<Result<IReadOnlyList<AdminPageSeo>>> Handle(ListAdminPageSeoQuery query,
        CancellationToken cancellationToken)
    {
        var rows = await db.PageSeo.AsNoTracking().ToDictionaryAsync(row => row.Page, cancellationToken);
        IReadOnlyList<AdminPageSeo> pages = SitePages.All
            .Select(page => PageSeoMapper.ToAdmin(page, rows.GetValueOrDefault(page)))
            .ToList();
        return Result.Success(pages);
    }
}
