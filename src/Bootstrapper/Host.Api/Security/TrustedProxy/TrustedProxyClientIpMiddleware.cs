using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace Host.Api.Security.TrustedProxy;

/// <summary>
/// Restores the real client IP when the request comes through the Cloudflare API proxy.
/// The proxy header is trusted only together with the shared secret, so a direct call to
/// the Cloud Run URL cannot spoof the IP used by rate limiting and audit logs.
/// </summary>
public sealed class TrustedProxyClientIpMiddleware(
    RequestDelegate next,
    IOptions<TrustedProxyOptions> options)
{
    private readonly TrustedProxyOptions _options = options.Value;

    public Task InvokeAsync(HttpContext context)
    {
        var headers = context.Request.Headers;

        if (_options.IsEnabled &&
            IsTrustedProxyRequest(headers) &&
            IPAddress.TryParse(headers[_options.ClientIpHeaderName].ToString(), out var clientIp))
        {
            context.Connection.RemoteIpAddress = clientIp;
        }

        headers.Remove(_options.SecretHeaderName);
        headers.Remove(_options.ClientIpHeaderName);

        return next(context);
    }

    private bool IsTrustedProxyRequest(IHeaderDictionary headers)
    {
        var providedSecret = headers[_options.SecretHeaderName].ToString();
        if (string.IsNullOrEmpty(providedSecret))
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(providedSecret),
            Encoding.UTF8.GetBytes(_options.SharedSecret!));
    }
}
