namespace Content.Application.Features.Overview.Queries.GetAdminContentOverview;

/// <summary>Site content problems for the admin «Огляд»: only what is waiting for someone.</summary>
/// <param name="ArticleDrafts">Articles not published yet.</param>
/// <param name="OldestArticleDraft">The draft edited longest ago, or <c>null</c> without drafts.</param>
/// <param name="LegacyArticleCovers">Articles whose cover is still stored in the database, not in the media library.</param>
/// <param name="DocumentDrafts">Unpublished document versions (at most one per document).</param>
public sealed record AdminContentOverview(
    int ArticleDrafts,
    AdminContentOverviewDraft? OldestArticleDraft,
    int LegacyArticleCovers,
    int DocumentDrafts);

public sealed record AdminContentOverviewDraft(long Id, string Title, DateTimeOffset UpdatedAt);
