using System.Net;
using Catalog.Application.Contracts.Integrations;
using Catalog.Application.Jobs.ProductPhotos;
using Catalog.Domain.Enums;

namespace Catalog.Infrastructure.Integrations.ProductPhotos;

public sealed class ProductPhotoProbe(
    HttpClient httpClient,
    IOptions<ProductPhotoCheckOptions> options) : IProductPhotoProbe
{
    private readonly ProductPhotoCheckOptions _options = Validate(options.Value);

    public async Task<ProductPhotoProbeResult> ProbeAsync(
        string photoUrl,
        CancellationToken cancellationToken)
    {
        if (!TryGetAllowedUri(photoUrl, out var uri))
            return Failed(null, "invalid_or_disallowed_url", "The photo URL is not an allowed HTTPS CDN URL.");

        using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutSource.CancelAfter(TimeSpan.FromSeconds(_options.RequestTimeoutSeconds));

        try
        {
            using var headResponse = await SendFollowingRedirectsAsync(
                HttpMethod.Head,
                uri,
                timeoutSource.Token);

            if (headResponse.StatusCode is HttpStatusCode.MethodNotAllowed or HttpStatusCode.NotImplemented)
            {
                using var getResponse = await SendFollowingRedirectsAsync(
                    HttpMethod.Get,
                    uri,
                    timeoutSource.Token);
                return Classify(getResponse);
            }

            return Classify(headResponse);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return Failed(null, "timeout", "The CDN request timed out.");
        }
        catch (HttpRequestException exception)
        {
            return Failed(
                exception.StatusCode is null ? null : (int)exception.StatusCode.Value,
                "network_error",
                "The CDN request failed before a usable response was received.");
        }
    }

    private async Task<HttpResponseMessage> SendFollowingRedirectsAsync(
        HttpMethod method,
        Uri initialUri,
        CancellationToken cancellationToken)
    {
        var currentUri = initialUri;

        for (var redirectCount = 0; ; redirectCount++)
        {
            using var request = new HttpRequestMessage(method, currentUri);
            var response = await httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            if (!IsRedirect(response.StatusCode))
                return response;

            if (redirectCount >= _options.MaxRedirects || response.Headers.Location is null)
            {
                response.Dispose();
                throw new HttpRequestException("The CDN redirect limit was exceeded or a redirect target was missing.");
            }

            var redirectUri = response.Headers.Location.IsAbsoluteUri
                ? response.Headers.Location
                : new Uri(currentUri, response.Headers.Location);
            response.Dispose();

            if (!TryGetAllowedUri(redirectUri.AbsoluteUri, out currentUri))
                throw new HttpRequestException("The CDN redirected to a disallowed URL.");
        }
    }

    private ProductPhotoProbeResult Classify(HttpResponseMessage response)
    {
        var statusCode = (int)response.StatusCode;
        if (response.IsSuccessStatusCode)
        {
            var mediaType = response.Content.Headers.ContentType?.MediaType;
            if (mediaType?.StartsWith("image/", StringComparison.OrdinalIgnoreCase) == true)
                return new(ProductPhotoStatus.Available, statusCode, null, null);

            return new(
                ProductPhotoStatus.InvalidContentType,
                statusCode,
                "invalid_content_type",
                string.IsNullOrWhiteSpace(mediaType) ? "Content-Type is missing." : mediaType);
        }

        if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Gone)
            return new(ProductPhotoStatus.Missing, statusCode, "http_missing", null);

        return Failed(statusCode, $"http_{statusCode}", "The CDN returned a non-success response.");
    }

    private bool TryGetAllowedUri(string value, out Uri uri)
    {
        if (!Uri.TryCreate(value?.Trim(), UriKind.Absolute, out uri!) ||
            !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return _options.AllowedHosts.Contains(uri.Host, StringComparer.OrdinalIgnoreCase);
    }

    private static bool IsRedirect(HttpStatusCode statusCode)
        => statusCode is HttpStatusCode.MovedPermanently
            or HttpStatusCode.Found
            or HttpStatusCode.SeeOther
            or HttpStatusCode.TemporaryRedirect
            or HttpStatusCode.PermanentRedirect;

    private static ProductPhotoProbeResult Failed(int? statusCode, string code, string message)
        => new(ProductPhotoStatus.CheckFailed, statusCode, code, message);

    private static ProductPhotoCheckOptions Validate(ProductPhotoCheckOptions options)
    {
        if (options.RequestTimeoutSeconds is < 1 or > 120)
            throw new InvalidOperationException("ProductPhotoCheck:RequestTimeoutSeconds must be between 1 and 120.");
        if (options.MaxRedirects is < 0 or > 10)
            throw new InvalidOperationException("ProductPhotoCheck:MaxRedirects must be between 0 and 10.");
        if (options.AllowedHosts.Length == 0 || options.AllowedHosts.Any(string.IsNullOrWhiteSpace))
            throw new InvalidOperationException("ProductPhotoCheck:AllowedHosts must contain at least one host.");

        return options;
    }
}
