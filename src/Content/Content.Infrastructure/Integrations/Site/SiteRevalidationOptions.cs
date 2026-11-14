namespace Content.Infrastructure.Integrations.Site;

/// <summary>
/// Where the public site takes cache refresh requests (<c>SiteRevalidation:Url</c>, the site origin) and the secret it
/// checks (<c>SiteRevalidation:Secret</c>, the same value as the site's <c>KEDR_REVALIDATE_SECRET</c>).
/// Both empty means the site is not connected yet.
/// </summary>
public sealed class SiteRevalidationOptions
{
    public const string SectionName = "SiteRevalidation";
    public const string SecretHeader = "x-revalidate-secret";

    public string? Url { get; set; }
    public string? Secret { get; set; }

    /// <summary>Cache tags the site drops: everything from the Content module and the catalog.</summary>
    public static readonly IReadOnlyList<string> Tags = ["content", "catalog"];
}
