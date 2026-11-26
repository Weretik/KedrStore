namespace Content.Application.Features.Redirects.Shared;

/// <summary>One redirect as the admin edits it.</summary>
public sealed record AdminSiteRedirect(long Id, string From, string To, DateTimeOffset UpdatedAt, string UpdatedBy,
    uint Version);

/// <summary>One page of the admin list, newest change first.</summary>
public sealed record AdminSiteRedirectPage(IReadOnlyList<AdminSiteRedirect> Items, int TotalCount, int Page,
    int PageSize);

/// <summary>What the site needs: the old path as it looks it up and where to send the visitor.</summary>
public sealed record PublicSiteRedirect(string From, string To);

internal static class SiteRedirectMapper
{
    public static AdminSiteRedirect ToAdmin(SiteRedirect redirect) => new(redirect.Id.Value, redirect.FromPath,
        redirect.ToUrl, redirect.UpdatedAt, redirect.UpdatedBy, redirect.Version);
}
