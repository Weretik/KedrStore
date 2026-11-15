using Content.Application.Features.SiteSeo.Shared;
namespace Content.Api.Endpoints.SiteSeo.Shared;

public sealed record PublicPageSeoResponse(string Page, string? Title, string? Description);

public sealed record AdminPageSeoResponse(
    string Page,
    string? TitleUk,
    string? TitleRu,
    string? DescriptionUk,
    string? DescriptionRu,
    DateTimeOffset? UpdatedAt,
    string? UpdatedBy,
    uint Version);

/// <summary>
/// An empty or missing value returns that text to the site default. <c>Version</c> is the one the form read (0 for a
/// page never saved): when the page changed since, the save answers 409; without it the save is not checked.
/// </summary>
public sealed record SavePageSeoRequest(
    string? TitleUk,
    string? TitleRu,
    string? DescriptionUk,
    string? DescriptionRu,
    uint? Version = null);

internal static class PageSeoResponseMapper
{
    public static IReadOnlyList<PublicPageSeoResponse> ToResponse(IReadOnlyList<PublicPageSeo> pages)
        => pages.Select(page => new PublicPageSeoResponse(page.Page, page.Title, page.Description)).ToList();

    public static IReadOnlyList<AdminPageSeoResponse> ToResponse(IReadOnlyList<AdminPageSeo> pages)
        => pages.Select(ToResponse).ToList();

    public static AdminPageSeoResponse ToResponse(AdminPageSeo page) => new(page.Page, page.TitleUk, page.TitleRu,
        page.DescriptionUk, page.DescriptionRu, page.UpdatedAt, page.UpdatedBy, page.Version);
}
