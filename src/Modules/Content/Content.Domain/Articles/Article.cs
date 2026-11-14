using System.Text.RegularExpressions;
using Content.Domain.Editing;

namespace Content.Domain.Articles;

/// <summary>
/// A site article in Ukrainian and Russian with one shared slug. Only published articles reach the site.
/// The cover is a media-library picture (<see cref="CoverMediaFileId"/>); covers saved before Content 008 are bytes in
/// <see cref="ArticleCover"/> referenced by <see cref="CoverVersion"/> until they are replaced.
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

    /// <summary>Version of a legacy cover kept in <see cref="ArticleCover"/>; <c>null</c> when there is none.</summary>
    public int? CoverVersion { get; private set; }

    /// <summary>The cover picture in the media library (CDN folder <c>articles</c>); <c>null</c> without one.</summary>
    public long? CoverMediaFileId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>Who saved the last change; empty for articles saved before this was recorded.</summary>
    public string? UpdatedBy { get; private set; }
    public IReadOnlyList<ArticleTranslation> Translations => _translations;

    private Article() { }

    public static DomainResult<Article> Create(string slug, DateOnly publishedOn, bool isPublished,
        IReadOnlyCollection<ArticleText> texts, DateTimeOffset now, string editor)
    {
        var article = new Article { CreatedAt = now };
        var updated = article.Update(slug, publishedOn, isPublished, texts, now, editor);
        return updated.IsFailure ? updated.Error! : article;
    }

    public DomainResult Update(string slug, DateOnly publishedOn, bool isPublished,
        IReadOnlyCollection<ArticleText> texts, DateTimeOffset now, string editor)
    {
        ArgumentNullException.ThrowIfNull(texts);
        var normalizedSlug = slug?.Trim() ?? string.Empty;
        if (!IsValidSlug(normalizedSlug))
            return ArticleErrors.SlugInvalid();

        var translations = new List<ArticleTranslation>();
        foreach (var text in texts)
        {
            var translation = ArticleTranslation.Create(text);
            if (translation.IsFailure)
                return translation.Error!;
            translations.Add(translation.Value);
        }

        foreach (var language in Languages)
        {
            if (translations.Count(t => t.Language == language) != 1)
                return ArticleErrors.LanguageMissing(language);
        }

        Slug = normalizedSlug;
        PublishedOn = publishedOn;
        IsPublished = isPublished;
        _translations.Clear();
        _translations.AddRange(translations.OrderBy(t => t.Language, StringComparer.Ordinal));
        Touch(now, editor);
        return DomainResult.Success();
    }

    public ArticleTranslation Text(string language) => _translations.Single(t => t.Language == language);

    /// <summary>Uses a media-library picture as the cover; a legacy cover stops being used.</summary>
    public DomainResult SetCover(long mediaFileId, DateTimeOffset now, string editor)
    {
        if (mediaFileId <= 0)
            return ArticleErrors.CoverInvalid();
        if (CoverMediaFileId == mediaFileId && CoverVersion is null)
            return DomainResult.Success();

        CoverMediaFileId = mediaFileId;
        CoverVersion = null;
        Touch(now, editor);
        return DomainResult.Success();
    }

    public void RemoveCover(DateTimeOffset now, string editor)
    {
        if (CoverMediaFileId is null && CoverVersion is null)
            return;

        CoverMediaFileId = null;
        CoverVersion = null;
        Touch(now, editor);
    }

    private void Touch(DateTimeOffset now, string editor)
    {
        UpdatedAt = now;
        UpdatedBy = EditorName.Normalize(editor);
    }

    public static bool IsValidSlug(string? slug)
        => slug is { Length: > 0 and <= SlugMaxLength } && SlugPattern().IsMatch(slug);

    [GeneratedRegex("^[a-z0-9]+(-[a-z0-9]+)*$", RegexOptions.CultureInvariant)]
    private static partial Regex SlugPattern();
}
