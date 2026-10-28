using Identity.Api.Options;

namespace Identity.Api.Cookies;

/// <summary>
/// Writes and checks the readable CSRF cookie that pairs with the browser session cookie.
/// </summary>
public sealed class SessionCookieWriter(IOptions<SessionCookieOptions> sessionCookieOptions)
{
    private readonly SessionCookieOptions _options = sessionCookieOptions.Value;

    public void AppendCsrfToken(HttpResponse response)
    {
        var csrfToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        response.Cookies.Append(_options.CsrfCookieName, csrfToken, new CookieOptions
        {
            HttpOnly = false,
            Secure = _options.Secure,
            SameSite = _options.SameSite,
            Path = _options.CsrfCookiePath,
            Domain = CsrfCookieDomain,
            MaxAge = TimeSpan.FromDays(_options.CsrfCookieTtlDays),
            IsEssential = true
        });
    }

    /// <summary>Issues a CSRF cookie only when the browser has none, for example after it expired.</summary>
    public void EnsureCsrfToken(HttpRequest request, HttpResponse response)
    {
        if (!request.Cookies.ContainsKey(_options.CsrfCookieName))
        {
            AppendCsrfToken(response);
        }
    }

    public void DeleteCsrfToken(HttpResponse response)
    {
        response.Cookies.Delete(_options.CsrfCookieName, new CookieOptions
        {
            Path = _options.CsrfCookiePath,
            Domain = CsrfCookieDomain,
            Secure = _options.Secure,
            SameSite = _options.SameSite,
            HttpOnly = false,
            IsEssential = true
        });
    }

    public bool HasValidCsrfToken(HttpRequest request)
    {
        if (!request.Cookies.TryGetValue(_options.CsrfCookieName, out var csrfCookie) || string.IsNullOrWhiteSpace(csrfCookie))
        {
            return false;
        }

        if (!request.Headers.TryGetValue(_options.CsrfHeaderName, out var csrfHeader) || string.IsNullOrWhiteSpace(csrfHeader))
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(
            System.Text.Encoding.UTF8.GetBytes(csrfCookie),
            System.Text.Encoding.UTF8.GetBytes(csrfHeader.ToString()));
    }

    private string? CsrfCookieDomain =>
        string.IsNullOrWhiteSpace(_options.CsrfCookieDomain) ? null : _options.CsrfCookieDomain;
}
