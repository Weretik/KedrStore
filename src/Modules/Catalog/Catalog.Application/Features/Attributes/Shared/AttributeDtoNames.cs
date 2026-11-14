using Catalog.Domain.Attributes;

namespace Catalog.Application.Features.Attributes.Shared;

/// <summary>The lower-case names of attribute enums in the admin API.</summary>
internal static class AttributeDtoNames
{
    public static string Kind(AttributeKind kind) => kind switch
    {
        AttributeKind.Number => "number",
        AttributeKind.Size => "size",
        _ => "enum"
    };

    public static string Source(AttributeValueSource source) => source switch
    {
        AttributeValueSource.Parser => "parser",
        AttributeValueSource.Manual => "manual",
        _ => "import"
    };

    public static string State(AttributeReviewState state) => state switch
    {
        AttributeReviewState.NeedsReview => "needs_review",
        AttributeReviewState.Accepted => "accepted",
        _ => "clear"
    };
}
