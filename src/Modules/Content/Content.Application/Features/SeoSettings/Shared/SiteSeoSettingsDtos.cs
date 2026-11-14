using Content.Application.Abstractions.Media;

namespace Content.Application.Features.SeoSettings.Shared;

/// <summary>Admin input; every field is optional and a blank one removes the code.</summary>
public sealed record SiteSeoSettingsInput(
    string? GoogleAnalyticsId,
    string? GoogleTagManagerId,
    string? GoogleSiteVerification,
    string? BingSiteVerification,
    long? ShareImageMediaFileId);

/// <summary>
/// The picture for link previews. <c>Missing</c> marks a file deleted from the media library: the site then keeps its
/// own picture, and the admin shows the problem.
/// </summary>
public sealed record SiteShareImage(long MediaFileId, string? Url, string? FileName, int? Width, int? Height,
    bool Missing);

public sealed record SiteSeoSettingsDetails(
    string? GoogleAnalyticsId,
    string? GoogleTagManagerId,
    string? GoogleSiteVerification,
    string? BingSiteVerification,
    SiteShareImage? ShareImage,
    DateTimeOffset UpdatedAt,
    string? UpdatedBy);

internal static class SiteSeoSettingsMapper
{
    public static SiteSeoSettingsValues ToValues(SiteSeoSettingsInput input) => new(
        input.GoogleAnalyticsId, input.GoogleTagManagerId, input.GoogleSiteVerification, input.BingSiteVerification,
        input.ShareImageMediaFileId);

    /// <summary>The settings with their picture looked up in the media library.</summary>
    public static async Task<SiteSeoSettingsDetails> ToDetailsAsync(SiteSeoSettings settings,
        IMediaImageReader images, CancellationToken cancellationToken)
    {
        SiteShareImage? shareImage = null;
        if (settings.ShareImageMediaFileId is { } id)
        {
            var files = await images.GetImagesAsync([id], cancellationToken);
            shareImage = files.TryGetValue(id, out var image)
                ? new SiteShareImage(id, image.Url, image.FileName, image.Width, image.Height, false)
                : new SiteShareImage(id, null, null, null, null, true);
        }

        return new SiteSeoSettingsDetails(settings.GoogleAnalyticsId, settings.GoogleTagManagerId,
            settings.GoogleSiteVerification, settings.BingSiteVerification, shareImage, settings.UpdatedAt,
            settings.UpdatedBy);
    }
}
