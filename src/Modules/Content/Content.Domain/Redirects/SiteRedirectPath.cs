namespace Content.Domain.Redirects;

/// <summary>
/// The address rules of a redirect. An old address is stored as the site matches it: the path alone (a full URL
/// gives its path, a query or fragment is dropped), percent-decoded, lower case, without a trailing slash. A new
/// address is a site path (its query kept) or a full http(s) URL.
/// </summary>
public static class SiteRedirectPath
{
    public const int FromMaxLength = 500;
    public const int ToMaxLength = 1000;

    private static readonly string[] ReservedPrefixes = ["/api/", "/_next/"];
    private static readonly string[] ReservedPaths = ["/", "/uk", "/ru", "/api", "/_next"];

    /// <summary>The old address as the site looks it up.</summary>
    public static DomainResult<string> NormalizeFrom(string? value)
    {
        const string field = "from";
        var text = value?.Trim();
        if (string.IsNullOrEmpty(text))
            return SiteRedirectErrors.Required(field);
        if (IsAbsoluteUrl(text, out var url))
            text = url.AbsolutePath;
        if (!IsSitePath(text))
            return SiteRedirectErrors.NotAPath(field);

        var path = LookupKey(text);
        if (path.Length > FromMaxLength)
            return SiteRedirectErrors.TooLong(field, FromMaxLength);
        if (IsReserved(path))
            return SiteRedirectErrors.ReservedPath(field);
        return path;
    }

    /// <summary>The new address: a site path with its query, or a full http(s) URL, trimmed.</summary>
    public static DomainResult<string> NormalizeTo(string? value)
    {
        const string field = "to";
        var text = value?.Trim();
        if (string.IsNullOrEmpty(text))
            return SiteRedirectErrors.Required(field);
        if (text.Length > ToMaxLength)
            return SiteRedirectErrors.TooLong(field, ToMaxLength);
        if (IsAbsoluteUrl(text, out _))
            return text.Any(char.IsWhiteSpace) ? SiteRedirectErrors.NotAPath(field) : text;
        if (!IsSitePath(text))
            return SiteRedirectErrors.NotAPath(field);

        var cut = text.IndexOfAny(['?', '#']);
        var path = cut < 0 ? text : text[..cut];
        var rest = cut < 0 ? string.Empty : text[cut..];
        var trimmed = path.Length > 1 ? path.TrimEnd('/') : path;
        return (trimmed.Length == 0 ? "/" : trimmed) + rest;
    }

    /// <summary>
    /// The key a new address would have as an old one, to refuse chains; <c>null</c> for a full URL, which the site
    /// never looks up again.
    /// </summary>
    public static string? KeyOfTarget(string to) => to.StartsWith('/') ? LookupKey(to) : null;

    /// <summary>A request path as the site looks it up: no query, decoded, lower case, no trailing slash.</summary>
    public static string LookupKey(string path)
    {
        var cut = path.IndexOfAny(['?', '#']);
        var bare = cut < 0 ? path : path[..cut];
        var decoded = Uri.UnescapeDataString(bare).ToLowerInvariant();
        var trimmed = decoded.TrimEnd('/');
        return trimmed.Length == 0 ? "/" : trimmed;
    }

    private static bool IsReserved(string path) =>
        ReservedPaths.Contains(path, StringComparer.Ordinal)
        || ReservedPrefixes.Any(prefix => path.StartsWith(prefix, StringComparison.Ordinal));

    private static bool IsSitePath(string text) =>
        text.StartsWith('/') && !text.StartsWith("//", StringComparison.Ordinal) && !text.Any(char.IsWhiteSpace);

    private static bool IsAbsoluteUrl(string text, out Uri url)
    {
        url = null!;
        if (!Uri.TryCreate(text, UriKind.Absolute, out var parsed))
            return false;
        if (parsed.Scheme != Uri.UriSchemeHttps && parsed.Scheme != Uri.UriSchemeHttp)
            return false;
        url = parsed;
        return true;
    }
}
