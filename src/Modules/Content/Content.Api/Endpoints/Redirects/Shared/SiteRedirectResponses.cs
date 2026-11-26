using Content.Application.Features.Redirects.Shared;

namespace Content.Api.Endpoints.Redirects.Shared;

public sealed record AdminSiteRedirectResponse(long Id, string From, string To, DateTimeOffset UpdatedAt,
    string UpdatedBy, uint Version);

public sealed record AdminSiteRedirectPageResponse(IReadOnlyList<AdminSiteRedirectResponse> Items, int TotalCount,
    int Page, int PageSize);

public sealed record PublicSiteRedirectResponse(string From, string To);

/// <summary>
/// <c>From</c> is an old site path or a full URL of it (its path is kept); <c>To</c> is a site path or a full http(s)
/// URL. <c>Version</c> is the one the form read: when the redirect changed since, the save answers 409.
/// </summary>
public sealed record SaveSiteRedirectRequest(string? From, string? To, uint? Version = null);

internal static class SiteRedirectResponseMapper
{
    public static AdminSiteRedirectResponse ToResponse(AdminSiteRedirect redirect) => new(redirect.Id, redirect.From,
        redirect.To, redirect.UpdatedAt, redirect.UpdatedBy, redirect.Version);

    public static AdminSiteRedirectPageResponse ToResponse(AdminSiteRedirectPage page) => new(
        page.Items.Select(ToResponse).ToList(), page.TotalCount, page.Page, page.PageSize);

    public static IReadOnlyList<PublicSiteRedirectResponse> ToResponse(IReadOnlyList<PublicSiteRedirect> redirects)
        => redirects.Select(redirect => new PublicSiteRedirectResponse(redirect.From, redirect.To)).ToList();
}
