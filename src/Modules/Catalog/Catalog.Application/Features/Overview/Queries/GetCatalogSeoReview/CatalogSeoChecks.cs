using Catalog.Contracts.Overview;

namespace Catalog.Application.Features.Overview.Queries.GetCatalogSeoReview;

/// <summary>One page in one language as the review sees it; <c>HasText</c> is the page's own body text.</summary>
internal sealed record CatalogSeoPage(string Kind, long Id, string Name, string Language, string? Title,
    string? Description, bool HasText);

/// <summary>
/// The search text rules of the review (spec platform/010 R-02): a title over 70 characters and a description over
/// 160 are cut in search results; a page without its own text or description; the same title on two pages of one kind
/// and language. Content has the same rules for articles.
/// </summary>
internal static class CatalogSeoChecks
{
    public const int TitleMaxLength = 70;
    public const int DescriptionMaxLength = 160;

    public static IReadOnlyList<SeoReviewItemResponse> Review(IReadOnlyList<CatalogSeoPage> pages)
    {
        var duplicates = pages.Where(page => !string.IsNullOrWhiteSpace(page.Title))
            .GroupBy(page => (page.Kind, page.Language, Title: page.Title!.Trim().ToLowerInvariant()))
            .Where(group => group.Count() > 1)
            .SelectMany(group => group)
            .ToHashSet();

        return pages.Select(page => new SeoReviewItemResponse(page.Kind, page.Id, page.Name, page.Language,
                Issues(page, duplicates.Contains(page))))
            .Where(item => item.Issues.Count > 0)
            .ToList();
    }

    private static List<string> Issues(CatalogSeoPage page, bool duplicate)
    {
        var issues = new List<string>();
        if (!page.HasText)
            issues.Add("noText");
        if (string.IsNullOrWhiteSpace(page.Description))
            issues.Add("noDescription");
        if (page.Title?.Trim().Length > TitleMaxLength)
            issues.Add("longTitle");
        if (page.Description?.Trim().Length > DescriptionMaxLength)
            issues.Add("longDescription");
        if (duplicate)
            issues.Add("duplicateTitle");
        return issues;
    }
}
