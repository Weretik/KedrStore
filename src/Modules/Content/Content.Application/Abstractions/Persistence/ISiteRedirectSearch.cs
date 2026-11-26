namespace Content.Application.Abstractions.Persistence;

/// <summary>
/// The admin search of site redirects: every word must appear in the old or the new address (escaped
/// <c>ILIKE</c> on trigram indexes, a PostgreSQL feature, so it lives in Infrastructure). Newest change first, paged
/// in SQL.
/// </summary>
public interface ISiteRedirectSearch
{
    Task<(IReadOnlyList<SiteRedirect> Items, int TotalCount)> SearchAsync(string? search, int page, int pageSize,
        CancellationToken cancellationToken);
}
