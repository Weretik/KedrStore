using Content.Application.Contracts.Html;
using Ganss.Xss;

namespace Content.Infrastructure.Html;

/// <summary>
/// Allow-list: p, br, strong, em, ul, ol, li, h2, h3, blockquote and a[href] with http, https, or a site path.
/// Other tags are removed but keep their text; script-like tags lose their content too.
/// </summary>
internal sealed class ArticleHtmlSanitizer : IArticleHtmlSanitizer
{
    private static readonly string[] AllowedTags =
        ["p", "br", "strong", "em", "ul", "ol", "li", "h2", "h3", "blockquote", "a"];

    private readonly HtmlSanitizer _sanitizer = CreateSanitizer();

    public string Sanitize(string html) => _sanitizer.Sanitize(html).Trim();

    private static HtmlSanitizer CreateSanitizer()
    {
        var sanitizer = new HtmlSanitizer(new HtmlSanitizerOptions
        {
            AllowedTags = new HashSet<string>(AllowedTags, StringComparer.OrdinalIgnoreCase),
            AllowedAttributes = new HashSet<string>(["href"], StringComparer.OrdinalIgnoreCase),
            AllowedSchemes = new HashSet<string>(["http", "https"], StringComparer.OrdinalIgnoreCase),
            UriAttributes = new HashSet<string>(["href"], StringComparer.OrdinalIgnoreCase),
            AllowedCssProperties = new HashSet<string>(),
            AllowedAtRules = new HashSet<AngleSharp.Css.Dom.CssRuleType>()
        })
        {
            KeepChildNodes = true
        };

        // Removed tags keep their text, but code inside script-like tags is not text a reader should see.
        sanitizer.RemovingTag += (_, args) =>
        {
            if (args.Tag.TagName is "SCRIPT" or "STYLE" or "TEMPLATE" or "NOSCRIPT" or "IFRAME" or "OBJECT")
                args.Tag.TextContent = string.Empty;
        };

        // Relative links must be site paths ("/..."), not protocol-relative ("//host") or page-relative ones.
        sanitizer.FilterUrl += (_, args) =>
        {
            var url = args.OriginalUrl.Trim();
            var isSitePath = url.StartsWith('/') && !url.StartsWith("//", StringComparison.Ordinal);
            var isAbsoluteWeb = url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                                url.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
            if (!isSitePath && !isAbsoluteWeb)
                args.SanitizedUrl = null;
        };

        return sanitizer;
    }
}
