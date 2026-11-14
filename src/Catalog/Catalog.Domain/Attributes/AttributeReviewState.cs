using Catalog.Domain.Groups;
using Catalog.Domain.Products.Media;
namespace Catalog.Domain.Attributes;

/// <summary>Whether a person has to look at the attributes recognized for a product.</summary>
public enum AttributeReviewState : short
{
    /// <summary>Everything was recognized.</summary>
    Clear = 0,

    /// <summary>Something is ambiguous or unknown; the reasons say what.</summary>
    NeedsReview = 1,

    /// <summary>A person accepted the product as it is. It stays accepted until a person reopens it.</summary>
    Accepted = 2
}
