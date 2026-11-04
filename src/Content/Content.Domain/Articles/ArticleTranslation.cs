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

    internal static ArticleTranslation Create(ArticleText text)
    {
        var language = text.Language?.Trim().ToLowerInvariant() ?? string.Empty;
        if (!Article.Languages.Contains(language))
            throw new DomainException(ArticleErrors.LanguageInvalid(text.Language));

        return new ArticleTranslation
        {
            Language = language,
            Title = Required(text.Title, TitleMaxLength, nameof(Title)),
            Excerpt = Required(text.Excerpt, ExcerptMaxLength, nameof(Excerpt)),
            BodyHtml = Required(text.BodyHtml, BodyHtmlMaxLength, nameof(BodyHtml)),
        };
    }

    private static string Required(string? value, int maxLength, string field)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException(ArticleErrors.Required(field));

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
            throw new DomainException(ArticleErrors.TooLong(field, maxLength, trimmed.Length));

        return trimmed;
    }
}

/// <summary>Input for one language; <see cref="BodyHtml"/> must already be sanitized.</summary>
public sealed record ArticleText(string Language, string Title, string Excerpt, string BodyHtml);
