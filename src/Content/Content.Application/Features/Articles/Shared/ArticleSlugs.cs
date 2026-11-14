using System.Text;
using BuildingBlocks.Application.Text;

namespace Content.Application.Features.Articles.Shared;

/// <summary>Builds a free article slug: transliterated Ukrainian title, then -2, -3… on a clash.</summary>
internal static class ArticleSlugs
{
    public static async Task<string> FreeFromTitleAsync(IReadContentDbContext db, string title, CancellationToken cancellationToken)
    {
        var baseSlug = FromTitle(title);
        for (var number = 1; ; number++)
        {
            var candidate = number == 1 ? baseSlug : Cut(baseSlug, Article.SlugMaxLength - 1 - number.ToString().Length) + "-" + number;
            if (!await db.Articles.AnyAsync(article => article.Slug == candidate, cancellationToken))
                return candidate;
        }
    }

    public static Task<bool> IsTakenAsync(IReadContentDbContext db, string slug, ArticleId? exceptId,
        CancellationToken cancellationToken)
        => exceptId is { } id
            ? db.Articles.AnyAsync(article => article.Slug == slug && article.Id != id, cancellationToken)
            : db.Articles.AnyAsync(article => article.Slug == slug, cancellationToken);

    public static Result<AdminArticleDetails> TakenResult(string slug) => Result.Invalid(new ValidationError(
        "slug", $"Адреса «{slug}» вже зайнята іншою статтею.", "Content.Article.SlugTaken", ValidationSeverity.Error));

    public static string FromTitle(string title)
    {
        var latin = UkrainianTransliterator.Transliterate(title);
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

        var slug = Cut(builder.ToString(), 80);
        return slug.Length == 0 ? "article" : slug;
    }

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
}
