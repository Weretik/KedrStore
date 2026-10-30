using Catalog.Application.Contracts.Content;
using Ganss.Xss;

namespace Catalog.Infrastructure.Content;

/// <summary>
/// Allow-list: p, br, strong, em, ul, ol, li, h2, h3 and a[href] with http, https, or a site-relative path.
/// Everything else is removed, not escaped.
/// </summary>
internal sealed class HtmlSanitizerService : IHtmlSanitizerService
{
    private static readonly string[] AllowedTags = ["p", "br", "strong", "em", "ul", "ol", "li", "h2", "h3", "a"];

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
