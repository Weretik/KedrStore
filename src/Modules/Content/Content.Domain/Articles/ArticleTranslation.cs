namespace Content.Domain.Articles;

/// <summary>Title, short description, and sanitized body of an article in one site language.</summary>
public sealed class ArticleTranslation
{
    public const int TitleMaxLength = 200;
    public const int ExcerptMaxLength = 300;
    public const int BodyHtmlMaxLength = 50_000;

    public string Language { get; private set; } = null!;
    public string Title { get; private set; } = null!;
    public string Excerpt { get; private set; } = null!;
    public string BodyHtml { get; private set; } = null!;

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
        if (checks.Error is { } error)
            return error;

        return new ArticleTranslation { Language = language, Title = title, Excerpt = excerpt, BodyHtml = bodyHtml };
    }

    private static DomainResult<string> Required(string? value, int maxLength, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
            return ArticleErrors.Required(field);

        var trimmed = value.Trim();
        return trimmed.Length > maxLength ? ArticleErrors.TooLong(field, maxLength, trimmed.Length) : trimmed;
    }
}

/// <summary>Input for one language; <see cref="BodyHtml"/> must already be sanitized.</summary>
public sealed record ArticleText(string Language, string Title, string Excerpt, string BodyHtml);
