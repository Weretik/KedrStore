namespace Content.Api.Endpoints.Articles.Shared;

/// <summary>
/// Article card on the site. <c>CoverUrl</c> is the absolute CDN address; <c>CoverPath</c> (relative to the API base
/// URL) is set only for a cover saved before Content 008. Both are null without a cover.
/// </summary>
public sealed record ArticleSummaryResponse(
    long Id, string Slug, string Title, string Excerpt, DateOnly PublishedOn, string? CoverPath, string? CoverUrl,
    DateTimeOffset UpdatedAt);

/// <summary>
/// Full article page; <c>BodyHtml</c> is sanitized on save. <c>SeoTitle</c> and <c>MetaDescription</c> are null when
/// not written; the site then uses the title and the excerpt (spec content/012 R-01).
/// </summary>
public sealed record ArticleResponse(
    long Id, string Slug, string Title, string Excerpt, string BodyHtml, DateOnly PublishedOn, string? CoverPath,
    string? CoverUrl, DateTimeOffset UpdatedAt, string? SeoTitle, string? MetaDescription);

public sealed record AdminArticleSummaryResponse(
    long Id, string Slug, string Title, DateOnly PublishedOn, bool IsPublished, string? CoverPath, string? CoverUrl,
    DateTimeOffset UpdatedAt, string? UpdatedBy);

/// <summary>Text of one language; the search title and description are optional (spec content/012 R-01).</summary>
public sealed record ArticleTextDto(string Language, string Title, string Excerpt, string BodyHtml,
    string? SeoTitle = null, string? MetaDescription = null);

/// <summary><c>CoverMediaFileId</c> stays set when the library file was deleted; then <c>CoverUrl</c> is null.</summary>
public sealed record AdminArticleResponse(
    long Id, string Slug, DateOnly PublishedOn, bool IsPublished, string? CoverPath, string? CoverUrl,
    long? CoverMediaFileId, DateTimeOffset UpdatedAt, string? UpdatedBy, IReadOnlyList<ArticleTextDto> Texts,
    uint Version);

/// <summary>
/// Create and update body. On create an empty slug is generated from the Ukrainian title. <c>CoverMediaFileId</c>
/// sets the cover (a file from <c>POST /api/admin/articles/covers</c> or the media library); <c>RemoveCover</c> drops
/// it; with neither the cover stays as it is. On update <c>Version</c> is the one the form read: when the article changed
/// since, the save answers 409; without it the save is not checked.
/// </summary>
public sealed record SaveArticleRequest(
    string? Slug, DateOnly PublishedOn, bool IsPublished, IReadOnlyList<ArticleTextDto> Texts,
    long? CoverMediaFileId = null, bool RemoveCover = false, uint? Version = null);

/// <summary>A cover just put into the media library (CDN folder <c>articles</c>).</summary>
public sealed record ArticleCoverUploadResponse(long MediaFileId, string Url);

public sealed record LegacyArticleCoversMovedResponse(int Moved);
