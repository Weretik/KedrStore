using Content.Application.Features.Redirects.Shared;

namespace Content.Application.Features.Redirects.Queries.ListPublicSiteRedirects;

/// <summary>Bounded by <see cref="SiteRedirect.MaxCount"/>, so the whole list is one small read.</summary>
public sealed class ListPublicSiteRedirectsHandler(IReadContentDbContext db)
    : IQueryHandler<ListPublicSiteRedirectsQuery, Result<IReadOnlyList<PublicSiteRedirect>>>
{
    public async ValueTask<Result<IReadOnlyList<PublicSiteRedirect>>> Handle(ListPublicSiteRedirectsQuery query,
        CancellationToken cancellationToken)
    {
        var items = await db.SiteRedirects.AsNoTracking()
            .OrderBy(redirect => redirect.FromPath)
            .Select(redirect => new PublicSiteRedirect(redirect.FromPath, redirect.ToUrl))
            .ToListAsync(cancellationToken);
        return Result.Success<IReadOnlyList<PublicSiteRedirect>>(items);
    }
}
