using System.Text.RegularExpressions;

namespace Content.Domain.Gallery;

/// <summary>
/// Reads the 11-character video ID from what a manager pastes: a watch, short, embed, shorts or live link,
/// with or without <c>www.</c>/<c>m.</c> and extra parameters, or the bare ID itself.
/// </summary>
public static partial class YoutubeVideoId
{
    public const int Length = 11;

    public static bool TryParse(string? value, out string id)
    {
        id = string.Empty;
        var text = value?.Trim();
        if (string.IsNullOrEmpty(text))
            return false;
        if (IdPattern().IsMatch(text))
        {
            id = text;
            return true;
        }

        if (!text.Contains("://", StringComparison.Ordinal))
            text = "https://" + text;
        if (!Uri.TryCreate(text, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
            return false;

        var host = uri.Host.ToLowerInvariant();
        var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        string? candidate = null;
        if (host == "youtu.be")
        {
            candidate = segments.FirstOrDefault();
        }
        else if (host is "youtube.com" or "www.youtube.com" or "m.youtube.com" or "music.youtube.com"
                 or "youtube-nocookie.com" or "www.youtube-nocookie.com")
        {
            if (segments.Length == 1 && segments[0] == "watch")
                candidate = QueryValue(uri.Query, "v");
            else if (segments.Length >= 2 && segments[0] is "embed" or "shorts" or "live" or "v")
                candidate = segments[1];
        }

        if (candidate is null || !IdPattern().IsMatch(candidate))
            return false;
        id = candidate;
        return true;
    }

    public static string Parse(string? value) => TryParse(value, out var id)
        ? id
        : throw new DomainException(GalleryErrors.YoutubeLinkInvalid(value));

    private static string? QueryValue(string query, string name)
    {
        foreach (var pair in query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var separator = pair.IndexOf('=');
            if (separator > 0 && pair[..separator] == name)
                return Uri.UnescapeDataString(pair[(separator + 1)..]);
        }

        return null;
    }

    [GeneratedRegex("^[A-Za-z0-9_-]{11}$")]
    private static partial Regex IdPattern();
}
