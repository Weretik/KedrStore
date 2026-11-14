namespace Content.Application.Abstractions.Integrations;

/// <summary>What happened when the site was asked to drop its cached content.</summary>
public enum SiteRefreshOutcome
{
    Refreshed,

    /// <summary>The site address or the shared secret is not set in this environment.</summary>
    NotConfigured,

    /// <summary>The site did not answer, refused the secret, or failed.</summary>
    SiteUnavailable,
}

/// <summary>Asks the public site to drop its cached content and catalog, so the next visit reads fresh data.</summary>
public interface ISiteCacheRefresher
{
    Task<SiteRefreshOutcome> RefreshAsync(CancellationToken cancellationToken);
}
