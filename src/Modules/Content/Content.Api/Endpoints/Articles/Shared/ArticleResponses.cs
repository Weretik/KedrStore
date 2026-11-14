namespace Content.Api.Endpoints.Articles.Shared;

/// <summary>
/// Article card on the site. <c>CoverUrl</c> is the absolute CDN address; <c>CoverPath</c> (relative to the API base
/// URL) is set only for a cover saved before Content 008. Both are null without a cover.
/// </summary>
public sealed record ArticleSummaryResponse(
    long Id, string Slug, string Title, string Excerpt, DateOnly PublishedOn, string? CoverPath, string? CoverUrl,
    DateTimeOffset UpdatedAt);

/// <summary>Full article page; <c>BodyHtml</c> is sanitized on save.</summary>
public sealed record ArticleResponse(
    long Id, string Slug, string Title, string Excerpt, string BodyHtml, DateOnly PublishedOn, string? CoverPath,
    string? CoverUrl, DateTimeOffset UpdatedAt);

public sealed record AdminArticleSummaryResponse(
    long Id, string Slug, string Title, DateOnly PublishedOn, bool IsPublished, string? CoverPath, string? CoverUrl,
    DateTimeOffset UpdatedAt, string? UpdatedBy);

public sealed record ArticleTextDto(string Language, string Title, string Excerpt, string BodyHtml);

/// <summary><c>CoverMediaFileId</c> stays set when the library file was deleted; then <c>CoverUrl</c> is null.</summary>
public sealed record AdminArticleResponse(
    long Id, string Slug, DateOnly PublishedOn, bool IsPublished, string? CoverPath, string? CoverUrl,
    long? CoverMediaFileId, DateTimeOffset UpdatedAt, string? UpdatedBy, IReadOnlyList<ArticleTextDto> Texts);

/// <summary>
/// Create and update body. On create an empty slug is generated from the Ukrainian title. <c>CoverMediaFileId</c>
/// sets the cover (a file from <c>POST /api/admin/articles/covers</c> or the media library); <c>RemoveCover</c> drops
/// it; with neither the cover stays as it is.
/// </summary>
public sealed record SaveArticleRequest(
    string? Slug, DateOnly PublishedOn, bool IsPublished, IReadOnlyList<ArticleTextDto> Texts,
    long? CoverMediaFileId = null, bool RemoveCover = false);

/// <summary>A cover just put into the media library (CDN folder <c>articles</c>).</summary>
public sealed record ArticleCoverUploadResponse(long MediaFileId, string Url);

public sealed record LegacyArticleCoversMovedResponse(int Moved);
