using System.Text.RegularExpressions;

namespace Content.Domain.Articles;

/// <summary>
/// A site article in Ukrainian and Russian with one shared slug. Only published articles reach the site;
/// the cover image lives in <see cref="ArticleCover"/> and is referenced by <see cref="CoverVersion"/>.
/// </summary>
public sealed partial class Article : IAggregateRoot
{
    public const int SlugMaxLength = 120;
    public static readonly IReadOnlyList<string> Languages = ["uk", "ru"];

    private readonly List<ArticleTranslation> _translations = [];

    public ArticleId Id { get; private set; }
    public string Slug { get; private set; } = null!;
    public DateOnly PublishedOn { get; private set; }
    public bool IsPublished { get; private set; }

    /// <summary>Changes with every new cover so its URL can be cached forever; <c>null</c> without a cover.</summary>
    public int? CoverVersion { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public IReadOnlyList<ArticleTranslation> Translations => _translations;

    private Article() { }

    public static Article Create(string slug, DateOnly publishedOn, bool isPublished,
        IReadOnlyCollection<ArticleText> texts, DateTimeOffset now)
    {
        var article = new Article { CreatedAt = now };
        article.Update(slug, publishedOn, isPublished, texts, now);
        return article;
    }

    public void Update(string slug, DateOnly publishedOn, bool isPublished,
        IReadOnlyCollection<ArticleText> texts, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(texts);
        var normalizedSlug = slug?.Trim() ?? string.Empty;
        if (!IsValidSlug(normalizedSlug))
            throw new DomainException(ArticleErrors.SlugInvalid(slug));

        var translations = texts.Select(ArticleTranslation.Create).ToList();
        foreach (var language in Languages)
        {
            if (translations.Count(t => t.Language == language) != 1)
                throw new DomainException(ArticleErrors.LanguageMissing(language));
        }

        Slug = normalizedSlug;
        PublishedOn = publishedOn;
        IsPublished = isPublished;
        _translations.Clear();
        _translations.AddRange(translations.OrderBy(t => t.Language, StringComparer.Ordinal));
        UpdatedAt = now;
    }

    public ArticleTranslation Text(string language) => _translations.Single(t => t.Language == language);

    public void ReplaceCover(DateTimeOffset now)
    {
        CoverVersion = (CoverVersion ?? 0) + 1;
        UpdatedAt = now;
    }

    public void RemoveCover(DateTimeOffset now)
    {
        CoverVersion = null;
        UpdatedAt = now;
    }

    public static bool IsValidSlug(string? slug)
        => slug is { Length: > 0 and <= SlugMaxLength } && SlugPattern().IsMatch(slug);

    [GeneratedRegex("^[a-z0-9]+(-[a-z0-9]+)*$", RegexOptions.CultureInvariant)]
    private static partial Regex SlugPattern();
}
