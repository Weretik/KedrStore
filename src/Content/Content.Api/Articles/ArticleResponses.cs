namespace Content.Api.Articles;

/// <summary>Article card on the site. <c>CoverPath</c> is relative to the API base URL; null without a cover.</summary>
public sealed record ArticleSummaryResponse(
    long Id, string Slug, string Title, string Excerpt, DateOnly PublishedOn, string? CoverPath, DateTimeOffset UpdatedAt);

/// <summary>Full article page; <c>BodyHtml</c> is sanitized on save.</summary>
public sealed record ArticleResponse(
    long Id, string Slug, string Title, string Excerpt, string BodyHtml, DateOnly PublishedOn, string? CoverPath,
    DateTimeOffset UpdatedAt);

public sealed record AdminArticleSummaryResponse(
    long Id, string Slug, string Title, DateOnly PublishedOn, bool IsPublished, string? CoverPath, DateTimeOffset UpdatedAt,
    string? UpdatedBy);

public sealed record ArticleTextDto(string Language, string Title, string Excerpt, string BodyHtml);

public sealed record AdminArticleResponse(
    long Id, string Slug, DateOnly PublishedOn, bool IsPublished, string? CoverPath, DateTimeOffset UpdatedAt,
    string? UpdatedBy, IReadOnlyList<ArticleTextDto> Texts);

/// <summary>Create and update body. On create an empty slug is generated from the Ukrainian title.</summary>
public sealed record SaveArticleRequest(string? Slug, DateOnly PublishedOn, bool IsPublished, IReadOnlyList<ArticleTextDto> Texts);
