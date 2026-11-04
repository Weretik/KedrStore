namespace Content.Application.Features.Documents.Common;

/// <summary>URL names of the document kinds (<c>public-offer</c>, <c>privacy-policy</c>, <c>returns-policy</c>).</summary>
public static class LegalDocumentKinds
{
    private static readonly IReadOnlyDictionary<string, LegalDocumentKind> BySlug =
        new Dictionary<string, LegalDocumentKind>(StringComparer.Ordinal)
        {
            ["public-offer"] = LegalDocumentKind.PublicOffer,
            ["privacy-policy"] = LegalDocumentKind.PrivacyPolicy,
            ["returns-policy"] = LegalDocumentKind.ReturnsPolicy,
        };

    public static IReadOnlyList<LegalDocumentKind> All { get; } = BySlug.Values.ToList();

    public static bool TryParse(string? slug, out LegalDocumentKind kind)
    {
        kind = default;
        return slug is not null && BySlug.TryGetValue(slug, out kind);
    }

    public static string ToSlug(LegalDocumentKind kind) => BySlug.First(pair => pair.Value == kind).Key;
}
