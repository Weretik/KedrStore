namespace Content.Application.Features.SiteSeo.Common;

/// <summary>One site page as the admin edits it; empty values mean the site keeps its own text.</summary>
public sealed record AdminPageSeo(
    string Page,
    string? TitleUk,
    string? TitleRu,
    string? DescriptionUk,
    string? DescriptionRu,
    DateTimeOffset? UpdatedAt,
    string? UpdatedBy);

/// <summary>One page in one language; only pages with at least one value are listed.</summary>
public sealed record PublicPageSeo(string Page, string? Title, string? Description);

internal static class PageSeoMapper
{
    public static AdminPageSeo ToAdmin(string page, PageSeo? seo) => seo is null
        ? new AdminPageSeo(page, null, null, null, null, null, null)
        : new AdminPageSeo(page, seo.TitleUk, seo.TitleRu, seo.DescriptionUk, seo.DescriptionRu, seo.UpdatedAt,
            seo.UpdatedBy);
}
