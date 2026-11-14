using Content.Application.Abstractions.Integrations;
using Content.Application.Features.Articles.Shared.Reading;
namespace Content.Application.Features.Articles.Shared.Writing;

internal static class ArticleTexts
{
    /// <summary>Turns admin input into domain texts with the body cleaned of anything but allowed formatting.</summary>
    public static IReadOnlyCollection<ArticleText> Sanitized(IEnumerable<ArticleTextInput> texts, IArticleHtmlSanitizer sanitizer)
        => texts.Select(text => new ArticleText(text.Language, text.Title, text.Excerpt, sanitizer.Sanitize(text.BodyHtml)))
            .ToList();
}
