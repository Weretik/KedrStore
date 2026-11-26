using BuildingBlocks.Application.Text;

namespace Content.Infrastructure.Persistence.Seo;

internal sealed class SiteRedirectSearch(ContentDbContext db) : ISiteRedirectSearch
{
    public async Task<(IReadOnlyList<SiteRedirect> Items, int TotalCount)> SearchAsync(string? search, int page,
        int pageSize, CancellationToken cancellationToken)
    {
        var redirects = db.SiteRedirects.AsNoTracking();
        foreach (var pattern in SearchWords.Patterns(search))
            redirects = redirects.Where(redirect =>
                EF.Functions.ILike(redirect.FromPath, pattern, SearchWords.EscapeCharacter)
                || EF.Functions.ILike(redirect.ToUrl, pattern, SearchWords.EscapeCharacter));

        var total = await redirects.CountAsync(cancellationToken);
        var items = await redirects.OrderByDescending(redirect => redirect.UpdatedAt)
            .ThenByDescending(redirect => redirect.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return (items, total);
    }
}
