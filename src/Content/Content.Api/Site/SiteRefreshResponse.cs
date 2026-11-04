namespace Content.Api.Site;

/// <summary>
/// Result of «Оновити сайт зараз»: <c>refreshed</c> (200), <c>notConfigured</c> (503, the site is not connected in
/// this environment) or <c>siteUnavailable</c> (502, the site did not accept the request).
/// </summary>
public sealed record SiteRefreshResponse(string Status);
