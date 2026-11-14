using Catalog.Domain.Categories.Content;
using Catalog.Domain.Products.Prices;
using Catalog.Domain.Attributes;
using Catalog.Domain.Groups;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;

namespace Catalog.Domain.Attributes;

/// <summary>
/// The recognition state of one product: its base name (the name without color, size, and side, used to group
/// variants) and the reasons a person has to look at it. The original 1C name is never changed.
/// </summary>
public class ProductAttributeReview : IAggregateRoot
{
    public const int BaseNameMaxLength = 300;
    public const int AcceptedByMaxLength = 64;
    public const int MaxReasons = 30;

    private List<AttributeReviewReason> _reasons = [];

    public ProductId ProductId { get; private set; }
    public string BaseName { get; private set; } = null!;
    public AttributeReviewState State { get; private set; }
    public IReadOnlyList<AttributeReviewReason> Reasons => _reasons;
    public string? AcceptedBy { get; private set; }
    public DateTimeOffset? AcceptedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    private ProductAttributeReview() { }

    public static ProductAttributeReview Create(ProductId productId, string baseName,
        IReadOnlyList<AttributeReviewReason> reasons, DateTimeOffset now)
    {
        var review = new ProductAttributeReview { ProductId = productId };
        review.Apply(baseName, reasons, now);
        return review;
    }

    /// <summary>
    /// Applies an automatic recognition result. An accepted product stays accepted (owner decision 2026-11-05);
    /// its reasons are still updated so a person sees them when reopening. Reasons owned by the group build stay.
    /// Returns whether anything changed.
    /// </summary>
    public bool ApplyAutomatic(string baseName, IReadOnlyList<AttributeReviewReason> reasons, DateTimeOffset now)
    {
        var normalized = Normalize(reasons.Where(reason => !AttributeReviewReason.IsOwnedByGroups(reason))
            .Concat(_reasons.Where(AttributeReviewReason.IsOwnedByGroups)).ToList());
        var name = AttributeText.Required(baseName, BaseNameMaxLength, nameof(BaseName));
        if (BaseName == name && _reasons.SequenceEqual(normalized) && State == NextState(normalized))
            return false;

        Apply(name, normalized, now);
        return true;
    }

    /// <summary>
    /// Sets or clears the «Однакові варіанти» reason found by the group build; <paramref name="text"/> names the
    /// matching products, <c>null</c> clears it. Returns whether anything changed.
    /// </summary>
    public bool SetSameVariants(string? text, DateTimeOffset now)
    {
        var others = _reasons.Where(reason => reason.Code != AttributeReviewReason.SameVariants);
        var wanted = Normalize((text is null
            ? others
            : others.Append(new AttributeReviewReason(AttributeReviewReason.SameVariants, text))).ToList());
        if (_reasons.SequenceEqual(wanted))
            return false;

        Apply(BaseName, wanted, now);
        return true;
    }

    /// <summary>A person accepts the product as it is; it no longer counts as a problem.</summary>
    public void Accept(string acceptedBy, DateTimeOffset now)
    {
        State = AttributeReviewState.Accepted;
        AcceptedBy = AttributeText.Required(acceptedBy, AcceptedByMaxLength, nameof(AcceptedBy));
        AcceptedAt = now;
        UpdatedAt = now;
    }

    /// <summary>A person returns an accepted product to review.</summary>
    public void Reopen(DateTimeOffset now)
    {
        AcceptedBy = null;
        AcceptedAt = null;
        State = _reasons.Count > 0 ? AttributeReviewState.NeedsReview : AttributeReviewState.Clear;
        UpdatedAt = now;
    }

    private void Apply(string baseName, IReadOnlyList<AttributeReviewReason> reasons, DateTimeOffset now)
    {
        BaseName = AttributeText.Required(baseName, BaseNameMaxLength, nameof(BaseName));
        _reasons = Normalize(reasons);
        State = NextState(_reasons);
        UpdatedAt = now;
    }

    private AttributeReviewState NextState(IReadOnlyList<AttributeReviewReason> reasons)
        => State == AttributeReviewState.Accepted
            ? AttributeReviewState.Accepted
            : reasons.Count > 0 ? AttributeReviewState.NeedsReview : AttributeReviewState.Clear;

    private static List<AttributeReviewReason> Normalize(IReadOnlyList<AttributeReviewReason> reasons)
        => reasons
            .Select(reason => new AttributeReviewReason(
                AttributeText.Required(reason.Code, AttributeReviewReason.CodeMaxLength, "ReasonCode"),
                AttributeText.Optional(reason.Text, AttributeReviewReason.TextMaxLength, "ReasonText")))
            .DistinctBy(reason => reason.Code)
            .Take(MaxReasons)
            .ToList();
}
