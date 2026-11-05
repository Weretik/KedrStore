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

    public static PageSeo Create(string page, PageSeoValues values, DateTimeOffset now, string editor)
    {
        if (!SitePages.IsKnown(page))
            throw new DomainException(PageSeoErrors.UnknownPage(page));

        var seo = new PageSeo { Page = page };
        seo.Update(values, now, editor);
        return seo;
    }

    public void Update(PageSeoValues values, DateTimeOffset now, string editor)
    {
        ArgumentNullException.ThrowIfNull(values);
        TitleUk = Clean(values.TitleUk, nameof(TitleUk), TitleMaxLength);
        TitleRu = Clean(values.TitleRu, nameof(TitleRu), TitleMaxLength);
        DescriptionUk = Clean(values.DescriptionUk, nameof(DescriptionUk), DescriptionMaxLength);
        DescriptionRu = Clean(values.DescriptionRu, nameof(DescriptionRu), DescriptionMaxLength);
        UpdatedAt = now;
        UpdatedBy = EditorName.Normalize(editor);
    }

    public string? Title(string language) => language switch
    {
        "uk" => TitleUk,
        "ru" => TitleRu,
        _ => throw new DomainException(PageSeoErrors.UnknownLanguage(language)),
    };

    public string? Description(string language) => language switch
    {
        "uk" => DescriptionUk,
        "ru" => DescriptionRu,
        _ => throw new DomainException(PageSeoErrors.UnknownLanguage(language)),
    };

    private static string? Clean(string? value, string field, int maxLength)
    {
        var text = value?.Trim();
        if (string.IsNullOrEmpty(text))
            return null;
        if (text.Length > maxLength)
            throw new DomainException(PageSeoErrors.TooLong(field, maxLength, text.Length));
        return text;
    }
}
