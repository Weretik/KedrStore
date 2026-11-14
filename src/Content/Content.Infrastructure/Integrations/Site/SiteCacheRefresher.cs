using System.Net.Http.Json;
using Content.Application.Abstractions.Integrations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Content.Infrastructure.Integrations.Site;

/// <summary>POSTs the cache tags to the site's <c>/api/revalidate</c> with the shared secret header.</summary>
internal sealed class SiteCacheRefresher(
    HttpClient http,
    IOptions<SiteRevalidationOptions> options,
    ILogger<SiteCacheRefresher> logger) : ISiteCacheRefresher
{
    public async Task<SiteRefreshOutcome> RefreshAsync(CancellationToken cancellationToken)
    {
        var settings = options.Value;
        if (string.IsNullOrWhiteSpace(settings.Secret)
            || !Uri.TryCreate(settings.Url, UriKind.Absolute, out var origin)
            || origin.Scheme is not ("https" or "http"))
            return SiteRefreshOutcome.NotConfigured;

        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(origin, "/api/revalidate"))
        {
            Content = JsonContent.Create(new { tags = SiteRevalidationOptions.Tags }),
        };
        request.Headers.Add(SiteRevalidationOptions.SecretHeader, settings.Secret);
        try
        {
            using var response = await http.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
                return SiteRefreshOutcome.Refreshed;
            logger.LogWarning("The site refused the cache refresh with {StatusCode}", (int)response.StatusCode);
            return SiteRefreshOutcome.SiteUnavailable;
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            logger.LogWarning(exception, "The site did not answer the cache refresh");
            return SiteRefreshOutcome.SiteUnavailable;
        }
    }
}
