namespace Content.Application.Features.Articles.Common;

/// <summary>Text of one language as the admin sends it; the body is HTML from the editor.</summary>
public sealed record ArticleTextInput(string Language, string Title, string Excerpt, string BodyHtml);

/// <summary>Create and update body. An empty slug on create is generated from the Ukrainian title.</summary>
public sealed record ArticleInput(
    string? Slug,
    DateOnly PublishedOn,
    bool IsPublished,
    IReadOnlyList<ArticleTextInput> Texts);

public sealed record AdminArticleListItem(
    long Id,
    string Slug,
    string Title,
    DateOnly PublishedOn,
    bool IsPublished,
    int? CoverVersion,
    DateTimeOffset UpdatedAt);

public sealed record AdminArticleDetails(
    long Id,
    string Slug,
    DateOnly PublishedOn,
    bool IsPublished,
    int? CoverVersion,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<ArticleTextInput> Texts);

public sealed record PublicArticleListItem(
    long Id,
    string Slug,
    string Title,
    string Excerpt,
    DateOnly PublishedOn,
    int? CoverVersion,
    DateTimeOffset UpdatedAt);

public sealed record PublicArticleDetails(
    long Id,
    string Slug,
    string Title,
    string Excerpt,
    string BodyHtml,
    DateOnly PublishedOn,
    int? CoverVersion,
    DateTimeOffset UpdatedAt);

public sealed record ArticleCoverFile(byte[] Data, string ContentType, int Version);
