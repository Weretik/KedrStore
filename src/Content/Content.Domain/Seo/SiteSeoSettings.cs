using System.Text.RegularExpressions;
using Content.Domain.Editing;

namespace Content.Domain.Seo;

/// <summary>What an editor saved; an empty value removes that code from the site.</summary>
public sealed record SiteSeoSettingsValues(
    string? GoogleAnalyticsId,
    string? GoogleTagManagerId,
    string? GoogleSiteVerification,
    string? BingSiteVerification,
    long? ShareImageMediaFileId);

/// <summary>
/// Site-wide SEO codes that every site page carries: the Google Analytics 4 and Tag Manager ids, the Google Search
/// Console and Bing Webmaster verification codes, and the picture shown when a page without its own picture is shared
/// (a media-library file ID without a cross-module key). There is exactly one row (<see cref="SingletonId"/>); the
/// migration creates it empty and the admin only replaces it. The SEO specialist connects the services; the site only
/// places the codes.
/// </summary>
public sealed partial class SiteSeoSettings : IAggregateRoot
{
    public const int SingletonId = 1;
    public const int AnalyticsIdMaxLength = 20;
    public const int TagManagerIdMaxLength = 20;
    public const int VerificationMaxLength = 100;

    public int Id { get; private set; }

    /// <summary>Google Analytics 4 measurement id, <c>G-XXXXXXXXXX</c>.</summary>
    public string? GoogleAnalyticsId { get; private set; }

    /// <summary>Google Tag Manager container id, <c>GTM-XXXXXXX</c>.</summary>
    public string? GoogleTagManagerId { get; private set; }

    /// <summary>The <c>content</c> of the <c>google-site-verification</c> meta tag.</summary>
    public string? GoogleSiteVerification { get; private set; }

    /// <summary>The <c>content</c> of the Bing <c>msvalidate.01</c> meta tag.</summary>
    public string? BingSiteVerification { get; private set; }

    /// <summary>Picture of the media library for link previews; the site keeps its own when empty.</summary>
    public long? ShareImageMediaFileId { get; private set; }

    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>Who saved the last change; empty for the row created by the migration.</summary>
    public string? UpdatedBy { get; private set; }

    private SiteSeoSettings() { }

    /// <summary>The empty row the migration inserts.</summary>
    public static SiteSeoSettings CreateEmpty(DateTimeOffset now) => new() { Id = SingletonId, UpdatedAt = now };

    public void Update(SiteSeoSettingsValues values, DateTimeOffset now, string editor)
    {
        ArgumentNullException.ThrowIfNull(values);
        GoogleAnalyticsId = Checked(NormalizeId(values.GoogleAnalyticsId), nameof(GoogleAnalyticsId), IsAnalyticsId);
        GoogleTagManagerId = Checked(NormalizeId(values.GoogleTagManagerId), nameof(GoogleTagManagerId), IsTagManagerId);
        GoogleSiteVerification = Checked(NormalizeVerification(values.GoogleSiteVerification),
            nameof(GoogleSiteVerification), IsVerification);
        BingSiteVerification = Checked(NormalizeVerification(values.BingSiteVerification),
            nameof(BingSiteVerification), IsVerification);
        if (values.ShareImageMediaFileId is <= 0)
            throw new DomainException(SiteSeoSettingsErrors.Invalid(nameof(ShareImageMediaFileId),
                values.ShareImageMediaFileId.ToString()));
        ShareImageMediaFileId = values.ShareImageMediaFileId;
        UpdatedAt = now;
        UpdatedBy = EditorName.Normalize(editor);
    }

    /// <summary>Trimmed and upper-cased; blank is null.</summary>
    public static string? NormalizeId(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToUpperInvariant();

    /// <summary>
    /// The code itself: the consoles show a whole meta tag, so its <c>content</c> is taken when one is pasted.
    /// Blank is null.
    /// </summary>
    public static string? NormalizeVerification(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        var text = value.Trim();
        var content = MetaContent().Match(text);
        return content.Success ? content.Groups["code"].Value.Trim() : text;
    }

    public static bool IsAnalyticsId(string value) => AnalyticsIdPattern().IsMatch(value);

    public static bool IsTagManagerId(string value) => TagManagerIdPattern().IsMatch(value);

    /// <summary>Letters, digits, <c>-</c> and <c>_</c>, as Google and Bing give them.</summary>
    public static bool IsVerification(string value) => VerificationPattern().IsMatch(value);

    private static string? Checked(string? value, string field, Func<string, bool> isValid) =>
        value is null || isValid(value)
            ? value
            : throw new DomainException(SiteSeoSettingsErrors.Invalid(field, value));

    [GeneratedRegex("^G-[A-Z0-9]{4,15}$", RegexOptions.CultureInvariant)]
    private static partial Regex AnalyticsIdPattern();

    [GeneratedRegex("^GTM-[A-Z0-9]{4,12}$", RegexOptions.CultureInvariant)]
    private static partial Regex TagManagerIdPattern();

    [GeneratedRegex("^[A-Za-z0-9_-]{8,100}$", RegexOptions.CultureInvariant)]
    private static partial Regex VerificationPattern();

    [GeneratedRegex("""content\s*=\s*["'](?<code>[^"']*)["']""", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase)]
    private static partial Regex MetaContent();
}
