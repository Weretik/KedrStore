namespace Content.Domain.Articles;

/// <summary>
/// Title, short description, sanitized body, and the optional search title and description (spec content/012 R-01) of
/// an article in one site language.
/// </summary>
public sealed class ArticleTranslation
{
    public const int TitleMaxLength = 200;
    public const int ExcerptMaxLength = 300;
    public const int BodyHtmlMaxLength = 50_000;
    public const int SeoTitleMaxLength = 70;
    public const int MetaDescriptionMaxLength = 160;

    public string Language { get; private set; } = null!;
    public string Title { get; private set; } = null!;
    public string Excerpt { get; private set; } = null!;
    public string BodyHtml { get; private set; } = null!;
    /// <summary>Search title; <c>null</c> lets the site use <see cref="Title"/>.</summary>
    public string? SeoTitle { get; private set; }
    /// <summary>Search description; <c>null</c> lets the site use <see cref="Excerpt"/>.</summary>
    public string? MetaDescription { get; private set; }

    private ArticleTranslation() { }

    internal static DomainResult<ArticleTranslation> Create(ArticleText text)
    {
        var language = text.Language?.Trim().ToLowerInvariant() ?? string.Empty;
        if (!Article.Languages.Contains(language))
            return ArticleErrors.LanguageInvalid();

        var checks = new DomainChecks();
        var title = checks.Take(Required(text.Title, TitleMaxLength, nameof(Title)));
        var excerpt = checks.Take(Required(text.Excerpt, ExcerptMaxLength, nameof(Excerpt)));
        var bodyHtml = checks.Take(Required(text.BodyHtml, BodyHtmlMaxLength, nameof(BodyHtml)));
        var seoTitle = checks.Take(Optional(text.SeoTitle, SeoTitleMaxLength, nameof(SeoTitle)));
        var metaDescription = checks.Take(Optional(text.MetaDescription, MetaDescriptionMaxLength, nameof(MetaDescription)));
        if (checks.Error is { } error)
            return error;

        return new ArticleTranslation
        {
            Language = language, Title = title, Excerpt = excerpt, BodyHtml = bodyHtml, SeoTitle = seoTitle,
            MetaDescription = metaDescription
        };
    }

    private static DomainResult<string?> Optional(string? value, int maxLength, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
            return (string?)null;

        var trimmed = value.Trim();
        return trimmed.Length > maxLength ? ArticleErrors.TooLong(field, maxLength, trimmed.Length) : trimmed;
    }

    private static DomainResult<string> Required(string? value, int maxLength, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ArticleErrors.Required(field);

        var trimmed = value.Trim();
        return trimmed.Length > maxLength ? ArticleErrors.TooLong(field, maxLength, trimmed.Length) : trimmed;
    }
}

/// <summary>Input for one language; <see cref="BodyHtml"/> must already be sanitized. Blank SEO fields are stored as null.</summary>
public sealed record ArticleText(string Language, string Title, string Excerpt, string BodyHtml,
    string? SeoTitle = null, string? MetaDescription = null);
