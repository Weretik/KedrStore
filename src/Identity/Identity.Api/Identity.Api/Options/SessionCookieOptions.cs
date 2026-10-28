namespace Identity.Api.Options;

public sealed class SessionCookieOptions
{
    public const string SectionName = "Identity:SessionCookies";

    public string CsrfCookieName { get; init; } = "kedr.csrf";
    public string CsrfHeaderName { get; init; } = "X-CSRF-Token";
    public string CsrfCookiePath { get; init; } = "/";
    // Shared parent domain so frontends can read the CSRF cookie; empty keeps it host-only.
    public string? CsrfCookieDomain { get; init; }
    public int CsrfCookieTtlDays { get; init; } = 14;

    public bool Secure { get; init; } = true;
    public SameSiteMode SameSite { get; init; } = SameSiteMode.None;
}
