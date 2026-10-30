using System.Text;
using System.Text.RegularExpressions;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Slugs;

/// <summary>Builds and validates the URL slug text of products and categories.</summary>
public static partial class SlugText
{
    public const int MaxLength = 80;

    public static string FromName(string? name, CatalogEntityType kind)
    {
        var latin = UkrainianTransliterator.Transliterate(name);
        var builder = new StringBuilder(latin.Length);
        var pendingHyphen = false;

        foreach (var character in latin)
        {
            if (character is >= 'a' and <= 'z' or >= '0' and <= '9')
            {
                if (pendingHyphen && builder.Length > 0)
                    builder.Append('-');

                builder.Append(character);
                pendingHyphen = false;
            }
            else
            {
                pendingHyphen = true;
            }
        }

        var slug = Cut(builder.ToString(), MaxLength);
        return slug.Length == 0 ? Fallback(kind) : slug;
    }

    public static string WithSuffix(string baseSlug, int number)
    {
        if (number <= 1)
            return baseSlug;

        var suffix = "-" + number;
        return Cut(baseSlug, MaxLength - suffix.Length) + suffix;
    }

    public static bool IsValid(string? slug)
        => slug is { Length: > 0 and <= MaxLength } && Pattern().IsMatch(slug);

    private static string Fallback(CatalogEntityType kind)
        => kind == CatalogEntityType.Category ? "category" : "product";

    private static string Cut(string slug, int maxLength)
    {
        if (slug.Length <= maxLength)
            return slug;

        var cut = slug[..maxLength];
        var lastHyphen = cut.LastIndexOf('-');
        if (slug[maxLength] != '-' && lastHyphen > 0)
            cut = cut[..lastHyphen];

        return cut.Trim('-');
    }

    [GeneratedRegex("^[a-z0-9]+(-[a-z0-9]+)*$", RegexOptions.CultureInvariant)]
    private static partial Regex Pattern();
}
