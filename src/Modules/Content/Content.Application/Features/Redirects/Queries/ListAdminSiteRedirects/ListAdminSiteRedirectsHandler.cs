using Content.Application.Features.Redirects.Shared;

namespace Content.Application.Features.Redirects.Queries.ListAdminSiteRedirects;

public sealed class ListAdminSiteRedirectsHandler(ISiteRedirectSearch search)
    : IQueryHandler<ListAdminSiteRedirectsQuery, Result<AdminSiteRedirectPage>>
{
    public async ValueTask<Result<AdminSiteRedirectPage>> Handle(ListAdminSiteRedirectsQuery query,
        CancellationToken cancellationToken)
    {
        var (items, total) = await search.SearchAsync(query.Search, query.Page, query.PageSize, cancellationToken);
        return Result.Success(new AdminSiteRedirectPage(items.Select(SiteRedirectMapper.ToAdmin).ToList(), total,
            query.Page, query.PageSize));
    }
}
