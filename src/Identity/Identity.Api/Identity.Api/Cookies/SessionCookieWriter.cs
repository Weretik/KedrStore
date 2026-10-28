using Identity.Api.Options;

namespace Identity.Api.Cookies;

/// <summary>
/// Writes and reads the refresh-token and CSRF cookies shared by the session endpoints.
/// </summary>
public sealed class SessionCookieWriter(IOptions<SessionCookieOptions> sessionCookieOptions)
{
    // Before the cookie path widened, the refresh cookie lived on the refresh path only.
    // Browsers send that more specific cookie first, so it must go once a new one is set.
    private const string LegacyRefreshTokenCookiePath = "/api/auth/session/refresh";

    private readonly SessionCookieOptions _options = sessionCookieOptions.Value;

    public string? ReadRefreshToken(HttpRequest request) =>
        request.Cookies.TryGetValue(_options.RefreshTokenCookieName, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : null;

    public void AppendRefreshToken(HttpResponse response, string refreshToken)
    {
        DeleteLegacyRefreshToken(response);
        response.Cookies.Append(_options.RefreshTokenCookieName, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = _options.Secure,
            SameSite = _options.SameSite,
            Path = _options.RefreshTokenCookiePath,
            MaxAge = TimeSpan.FromDays(_options.RefreshTokenCookieTtlDays),
            IsEssential = true
        });
    }

    public void DeleteRefreshToken(HttpResponse response)
    {
        response.Cookies.Delete(_options.RefreshTokenCookieName, RefreshDeleteOptions(_options.RefreshTokenCookiePath));
        DeleteLegacyRefreshToken(response);
    }

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

    private void DeleteLegacyRefreshToken(HttpResponse response)
    {
        if (!string.Equals(_options.RefreshTokenCookiePath, LegacyRefreshTokenCookiePath, StringComparison.Ordinal))
        {
            response.Cookies.Delete(_options.RefreshTokenCookieName, RefreshDeleteOptions(LegacyRefreshTokenCookiePath));
        }
    }

    private CookieOptions RefreshDeleteOptions(string path) => new()
    {
        Path = path,
        Secure = _options.Secure,
        SameSite = _options.SameSite,
        HttpOnly = true,
        IsEssential = true
    };

    private string? CsrfCookieDomain =>
        string.IsNullOrWhiteSpace(_options.CsrfCookieDomain) ? null : _options.CsrfCookieDomain;
}
