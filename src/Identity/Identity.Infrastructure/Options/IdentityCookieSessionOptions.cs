namespace Identity.Infrastructure.Options;

/// <summary>
/// Browser session cookie for Admin and the storefront.
/// </summary>
public sealed class IdentityCookieSessionOptions
{
    public const string SectionName = "Identity:CookieSession";

    // The __Host- prefix makes browsers require Secure, Path=/ and no Domain.
    public string CookieName { get; init; } = "__Host-kedr.session";
    public int LifetimeDays { get; init; } = 7;
    public int SecurityStampValidationMinutes { get; init; } = 5;
    // Only local HTTP development turns this off, together with a CookieName without the __Host- prefix.
    public bool RequireSecure { get; init; } = true;
}
