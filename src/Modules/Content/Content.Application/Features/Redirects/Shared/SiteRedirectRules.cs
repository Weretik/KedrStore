using BuildingBlocks.Domain.Abstractions.Errors;

namespace Content.Application.Features.Redirects.Shared;

/// <summary>
/// The rules that need the other redirects (spec content/013 R-03): one redirect per old address, and no chains: the
/// new address is not itself redirected, and no other redirect leads to the old address.
/// </summary>
internal static class SiteRedirectRules
{
    public static async Task<DomainError?> CheckAsync(IReadContentDbContext db, SiteRedirect redirect,
        CancellationToken cancellationToken)
    {
        var others = db.SiteRedirects.AsNoTracking().Where(other => other.Id != redirect.Id);
        if (await others.AnyAsync(other => other.FromPath == redirect.FromPath, cancellationToken))
            return SiteRedirectErrors.Duplicate();

        var target = SiteRedirectPath.KeyOfTarget(redirect.ToUrl);
        if (target is not null && await others.AnyAsync(other => other.FromPath == target, cancellationToken))
            return SiteRedirectErrors.Chain();

        // Bounded by SiteRedirect.MaxCount; a target's key is computed in code, so the paths are read once.
        var sitePaths = await others.Where(other => other.ToUrl.StartsWith("/"))
            .Select(other => other.ToUrl)
            .ToListAsync(cancellationToken);
        return sitePaths.Any(path => SiteRedirectPath.KeyOfTarget(path) == redirect.FromPath)
            ? SiteRedirectErrors.Chain()
            : null;
    }
}
