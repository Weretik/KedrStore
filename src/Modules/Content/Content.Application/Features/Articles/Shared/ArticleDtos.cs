namespace Content.Application.Features.Articles.Shared;

/// <summary>Text of one language as the admin sends it; the body is HTML from the editor.</summary>
public sealed record ArticleTextInput(string Language, string Title, string Excerpt, string BodyHtml);

/// <summary>
/// Create and update body. An empty slug on create is generated from the Ukrainian title.
/// <see cref="CoverMediaFileId"/> sets the cover; <see cref="RemoveCover"/> drops it; with neither the cover stays.
/// </summary>
public sealed record ArticleInput(
    string? Slug,
    DateOnly PublishedOn,
    bool IsPublished,
    IReadOnlyList<ArticleTextInput> Texts,
    long? CoverMediaFileId = null,
    bool RemoveCover = false);

/// <summary>
/// The cover as clients see it: <see cref="Url"/> on the CDN, or a <see cref="LegacyVersion"/> served by the API
/// for covers saved before Content 008. Both are <c>null</c> without a cover or when its file was deleted.
/// </summary>
public sealed record ArticleCoverView(long? MediaFileId, string? Url, int? LegacyVersion)
{
    public static readonly ArticleCoverView None = new(null, null, null);
}

/// <summary>A cover picture just put into the media library, ready to send with the article.</summary>
public sealed record UploadedArticleCover(long MediaFileId, string Url);

public sealed record AdminArticleListItem(
    long Id,
    string Slug,
    string Title,
    DateOnly PublishedOn,
    bool IsPublished,
    ArticleCoverView Cover,
    DateTimeOffset UpdatedAt,
    string? UpdatedBy);

public sealed record AdminArticleDetails(
    long Id,
    string Slug,
    DateOnly PublishedOn,
    bool IsPublished,
    ArticleCoverView Cover,
    DateTimeOffset UpdatedAt,
    string? UpdatedBy,
    IReadOnlyList<ArticleTextInput> Texts);

public sealed record PublicArticleListItem(
    long Id,
    string Slug,
    string Title,
    string Excerpt,
    DateOnly PublishedOn,
    ArticleCoverView Cover,
    DateTimeOffset UpdatedAt);

public sealed record PublicArticleDetails(
    long Id,
    string Slug,
    string Title,
    string Excerpt,
    string BodyHtml,
    DateOnly PublishedOn,
    ArticleCoverView Cover,
    DateTimeOffset UpdatedAt);

public sealed record ArticleCoverFile(byte[] Data, string ContentType, int Version);
