using Content.Application.Features.SiteSeo.Common;

namespace Content.Api.SiteSeo;

public sealed record PublicPageSeoResponse(string Page, string? Title, string? Description);

public sealed record AdminPageSeoResponse(
    string Page,
    string? TitleUk,
    string? TitleRu,
    string? DescriptionUk,
    string? DescriptionRu,
    DateTimeOffset? UpdatedAt,
    string? UpdatedBy);

/// <summary>An empty or missing value returns that text to the site default.</summary>
public sealed record SavePageSeoRequest(
    string? TitleUk,
    string? TitleRu,
    string? DescriptionUk,
    string? DescriptionRu);

internal static class PageSeoResponseMapper
{
    public static IReadOnlyList<PublicPageSeoResponse> ToResponse(IReadOnlyList<PublicPageSeo> pages)
        => pages.Select(page => new PublicPageSeoResponse(page.Page, page.Title, page.Description)).ToList();

    public static IReadOnlyList<AdminPageSeoResponse> ToResponse(IReadOnlyList<AdminPageSeo> pages)
        => pages.Select(ToResponse).ToList();

    public static AdminPageSeoResponse ToResponse(AdminPageSeo page) => new(page.Page, page.TitleUk, page.TitleRu,
        page.DescriptionUk, page.DescriptionRu, page.UpdatedAt, page.UpdatedBy);
}
