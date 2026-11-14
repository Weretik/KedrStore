using Content.Domain.Editing;

namespace Content.Domain.Seo;

/// <summary>What an editor wrote for one page; an empty value keeps the text the site has in code.</summary>
public sealed record PageSeoValues(string? TitleUk, string? TitleRu, string? DescriptionUk, string? DescriptionRu);

/// <summary>
/// The search title and description of one <see cref="SitePages">site page</see> in both languages. Every value is
/// optional: the site keeps its own text where none is set. A row appears with the first save of its page.
/// </summary>
public sealed class PageSeo : IAggregateRoot
{
    public const int TitleMaxLength = 120;
    public const int DescriptionMaxLength = 300;

    public string Page { get; private set; } = null!;
    public string? TitleUk { get; private set; }
    public string? TitleRu { get; private set; }
    public string? DescriptionUk { get; private set; }
    public string? DescriptionRu { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

    private PageSeo() { }

    public static DomainResult<PageSeo> Create(string page, PageSeoValues values, DateTimeOffset now, string editor)
    {
        if (!SitePages.IsKnown(page))
            return PageSeoErrors.UnknownPage(page);

        var seo = new PageSeo { Page = page };
        var updated = seo.Update(values, now, editor);
        return updated.IsSuccess ? seo : updated.Error!;
    }

    public DomainResult Update(PageSeoValues values, DateTimeOffset now, string editor)
    {
        ArgumentNullException.ThrowIfNull(values);
        var checks = new DomainChecks();
        var titleUk = checks.Take(Clean(values.TitleUk, nameof(TitleUk), TitleMaxLength));
        var titleRu = checks.Take(Clean(values.TitleRu, nameof(TitleRu), TitleMaxLength));
        var descriptionUk = checks.Take(Clean(values.DescriptionUk, nameof(DescriptionUk), DescriptionMaxLength));
        var descriptionRu = checks.Take(Clean(values.DescriptionRu, nameof(DescriptionRu), DescriptionMaxLength));
        if (checks.Error is { } error)
            return error;

        TitleUk = titleUk;
        TitleRu = titleRu;
        DescriptionUk = descriptionUk;
        DescriptionRu = descriptionRu;
        UpdatedAt = now;
        UpdatedBy = EditorName.Normalize(editor);
        return DomainResult.Success();
    }

    /// <summary>The title in a site language; callers pass a validated <c>uk</c> or <c>ru</c>.</summary>
    public string? Title(string language) => language switch
    {
        "uk" => TitleUk,
        "ru" => TitleRu,
        _ => throw new ArgumentOutOfRangeException(nameof(language), language, "Only uk and ru are site languages."),
    };

    /// <summary>The description in a site language; callers pass a validated <c>uk</c> or <c>ru</c>.</summary>
    public string? Description(string language) => language switch
    {
        "uk" => DescriptionUk,
        "ru" => DescriptionRu,
        _ => throw new ArgumentOutOfRangeException(nameof(language), language, "Only uk and ru are site languages."),
    };

    private static DomainResult<string?> Clean(string? value, string field, int maxLength)
    {
        var text = value?.Trim();
        if (string.IsNullOrEmpty(text))
            return DomainResult<string?>.Success(null);
        if (text.Length > maxLength)
            return PageSeoErrors.TooLong(field, maxLength, text.Length);
        return text;
    }
}
