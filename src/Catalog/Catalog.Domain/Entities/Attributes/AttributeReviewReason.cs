namespace Catalog.Domain.Entities.Attributes;

/// <summary>Why a product needs review: a stable code (for filters) and the recognizer's explanation, if any.</summary>
public sealed record AttributeReviewReason(string Code, string? Text)
{
    public const int CodeMaxLength = 64;
    public const int TextMaxLength = 1000;

    /// <summary>Product without brand marking (No name). Owner decision 2026-11-05: always reviewed by a person.</summary>
    public const string NoName = "no_name";

    /// <summary>
    /// The product does not differ from another variant of its group by color, size, or side («Однакові варіанти»).
    /// Owned by the group build: the recognizer import keeps it as it is.
    /// </summary>
    public const string SameVariants = "same_variants";

    public static bool IsOwnedByGroups(AttributeReviewReason reason) => reason.Code == SameVariants;
}
