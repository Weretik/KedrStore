namespace Identity.Api.Options;

public sealed class SessionCookieOptions
{
    public const string SectionName = "Identity:SessionCookies";

    public string RefreshTokenCookieName { get; init; } = "kedr.rt";
    public string RefreshTokenCookiePath { get; init; } = "/api/auth/session/refresh";
    public int RefreshTokenCookieTtlDays { get; init; } = 14;

    public string CsrfCookieName { get; init; } = "kedr.csrf";
    public string CsrfHeaderName { get; init; } = "X-CSRF-Token";
    public string CsrfCookiePath { get; init; } = "/";
    public int CsrfCookieTtlDays { get; init; } = 14;

    public bool Secure { get; init; } = true;
    public SameSiteMode SameSite { get; init; } = SameSiteMode.None;
}
