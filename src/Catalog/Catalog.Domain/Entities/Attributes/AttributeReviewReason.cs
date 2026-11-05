namespace Catalog.Domain.Entities.Attributes;

/// <summary>Why a product needs review: a stable code (for filters) and the recognizer's explanation, if any.</summary>
public sealed record AttributeReviewReason(string Code, string? Text)
{
    public const int CodeMaxLength = 64;
    public const int TextMaxLength = 1000;

    /// <summary>Product without brand marking (No name). Owner decision 2026-11-05: always reviewed by a person.</summary>
    public const string NoName = "no_name";
}
