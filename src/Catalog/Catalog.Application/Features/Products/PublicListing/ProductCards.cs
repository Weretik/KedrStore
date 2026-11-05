namespace Catalog.Application.Features.Products.PublicListing;

/// <summary>A product of a public list with its variant group, if any.</summary>
public readonly record struct CardCandidate(int ProductId, int? GroupId, int? MainProductId)
{
    /// <summary>A variant group is one card; a product without a group is its own card.</summary>
    public long CardKey => GroupId is { } groupId ? groupId : -(long)ProductId;
}

/// <summary>
/// Turns a sorted product list into cards (spec catalog/006, phase 5): one card per variant group, at the place of its
/// first product. The card shows the group's main product when it matches, otherwise the first matching variant.
/// </summary>
public static class ProductCards
{
    public static List<CardCandidate> Collapse(IEnumerable<CardCandidate> sorted)
    {
        var cards = new List<CardCandidate>();
        var indexByKey = new Dictionary<long, int>();
        foreach (var candidate in sorted)
        {
            if (!indexByKey.TryGetValue(candidate.CardKey, out var index))
            {
                indexByKey[candidate.CardKey] = cards.Count;
                cards.Add(candidate);
                continue;
            }

            if (candidate.MainProductId == candidate.ProductId)
                cards[index] = candidate;
        }

        return cards;
    }
}
