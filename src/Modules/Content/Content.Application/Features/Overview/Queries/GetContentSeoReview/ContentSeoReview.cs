namespace Content.Application.Features.Overview.Queries.GetContentSeoReview;

/// <summary>
/// One article in one language with what its search texts lack; <c>Issues</c> holds <c>noDescription</c>,
/// <c>longTitle</c>, <c>duplicateTitle</c>.
/// </summary>
public sealed record ContentSeoReviewItem(string Kind, long Id, string Name, string Language,
    IReadOnlyList<string> Issues);

/// <summary>The article page as the site titles it: its search title, else its title.</summary>
internal sealed record ContentSeoPage(long Id, string Name, string Language, string Title, string? Description);

/// <summary>
/// The search text rules of the review for articles, the same as Catalog's (spec platform/010 R-02): the page title
/// over 70 characters, no description of its own (the site then uses the excerpt), the same title on two articles.
/// An article always has a body, so there is no «no text» issue.
/// </summary>
internal static class ContentSeoChecks
{
    public const int TitleMaxLength = 70;

    public static IReadOnlyList<ContentSeoReviewItem> Review(IReadOnlyList<ContentSeoPage> pages)
    {
        var duplicates = pages.GroupBy(page => (page.Language, Title: page.Title.Trim().ToLowerInvariant()))
            .Where(group => group.Count() > 1)
            .SelectMany(group => group)
            .ToHashSet();

        return pages.Select(page => new ContentSeoReviewItem("article", page.Id, page.Name, page.Language,
                Issues(page, duplicates.Contains(page))))
            .Where(item => item.Issues.Count > 0)
            .ToList();
    }

    private static List<string> Issues(ContentSeoPage page, bool duplicate)
    {
        var issues = new List<string>();
        if (string.IsNullOrWhiteSpace(page.Description))
            issues.Add("noDescription");
        if (page.Title.Trim().Length > TitleMaxLength)
            issues.Add("longTitle");
        if (duplicate)
            issues.Add("duplicateTitle");
        return issues;
    }
}
