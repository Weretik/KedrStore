using Content.Application.Features.SeoSettings.Shared;

namespace Content.Api.Endpoints.SeoSettings;

/// <summary>A picture the site can use in link previews.</summary>
public sealed record SiteShareImageResponse(string Url, int? Width, int? Height);

/// <summary>
/// What the site places on every page. Empty codes are null; <c>shareImage</c> is null when none is chosen or its file
/// was deleted, and the site then keeps its own picture.
/// </summary>
public sealed record SiteSeoSettingsResponse(
    string? GoogleAnalyticsId,
    string? GoogleTagManagerId,
    string? GoogleSiteVerification,
    string? BingSiteVerification,
    SiteShareImageResponse? ShareImage);

/// <summary>The chosen picture as the admin sees it; <c>missing</c> marks a file deleted from the media library.</summary>
public sealed record AdminSiteShareImageResponse(
    long MediaFileId,
    string? Url,
    string? FileName,
    int? Width,
    int? Height,
    bool Missing);

/// <summary>The settings as the admin sees them, with who saved the last change (never shown on the site).</summary>
public sealed record AdminSiteSeoSettingsResponse(
    string? GoogleAnalyticsId,
    string? GoogleTagManagerId,
    string? GoogleSiteVerification,
    string? BingSiteVerification,
    AdminSiteShareImageResponse? ShareImage,
    DateTimeOffset UpdatedAt,
    string? UpdatedBy);

/// <summary>All fields are replaced at once; a blank or null field removes that code or picture.</summary>
public sealed record SaveSiteSeoSettingsRequest(
    string? GoogleAnalyticsId,
    string? GoogleTagManagerId,
    string? GoogleSiteVerification,
    string? BingSiteVerification,
    long? ShareImageMediaFileId);

internal static class SiteSeoSettingsResponseMapper
{
    public static SiteSeoSettingsResponse ToResponse(SiteSeoSettingsDetails details) => new(
        details.GoogleAnalyticsId, details.GoogleTagManagerId, details.GoogleSiteVerification,
        details.BingSiteVerification,
        details.ShareImage is { Missing: false, Url: { } url } image
            ? new SiteShareImageResponse(url, image.Width, image.Height)
            : null);

    public static AdminSiteSeoSettingsResponse ToAdminResponse(SiteSeoSettingsDetails details) => new(
        details.GoogleAnalyticsId, details.GoogleTagManagerId, details.GoogleSiteVerification,
        details.BingSiteVerification,
        details.ShareImage is { } image
            ? new AdminSiteShareImageResponse(image.MediaFileId, image.Url, image.FileName, image.Width, image.Height,
                image.Missing)
            : null,
        details.UpdatedAt, details.UpdatedBy);

    public static SiteSeoSettingsInput ToInput(SaveSiteSeoSettingsRequest request) => new(
        request.GoogleAnalyticsId, request.GoogleTagManagerId, request.GoogleSiteVerification,
        request.BingSiteVerification, request.ShareImageMediaFileId);
}
