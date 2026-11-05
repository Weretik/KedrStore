using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Products.PublicListing;

/// <summary>
/// Counts cards per filter value (spec catalog/006, phase 5). A value's count applies the choices of every other
/// attribute but not its own, so a buyer who chose «Хром» sees how many cards adding «Чорний» gives.
/// </summary>
public static class FacetCounter
{
    /// <param name="products">The list before attribute filters.</param>
    /// <param name="values">Confirmed values of each product.</param>
    /// <param name="filters">The chosen values.</param>
    /// <param name="attributes">The attributes to count.</param>
    public static FacetCounts Count(IReadOnlyList<CardCandidate> products,
        ILookup<int, (AttributeId Attribute, AttributeValueId Value)> values,
        IReadOnlyList<AttributeFilter> filters, IReadOnlyCollection<AttributeId> attributes)
    {
        var chosen = filters.ToDictionary(filter => filter.AttributeId, filter => filter.ValueIds.ToHashSet());
        var matched = products.Select(product => (Product: product,
                Missing: chosen.Where(pair => !values[product.ProductId]
                        .Any(value => value.Attribute == pair.Key && pair.Value.Contains(value.Value)))
                    .Select(pair => pair.Key).Take(2).ToList()))
            .ToList();

        var total = matched.Where(item => item.Missing.Count == 0).Select(item => item.Product.CardKey).Distinct().Count();
        var counts = new Dictionary<(AttributeId, AttributeValueId), HashSet<long>>();
        foreach (var (product, missing) in matched)
        {
            // A product counts for attribute A when every other attribute's choice matches.
            if (missing.Count > 1)
                continue;

            foreach (var (attribute, value) in values[product.ProductId])
            {
                if (!attributes.Contains(attribute) || (missing.Count == 1 && missing[0] != attribute))
                    continue;

                if (!counts.TryGetValue((attribute, value), out var cards))
                    counts[(attribute, value)] = cards = [];
                cards.Add(product.CardKey);
            }
        }

        return new FacetCounts(total, counts.ToDictionary(pair => pair.Key, pair => pair.Value.Count));
    }
}

/// <summary>Cards with every choice applied, and cards per (attribute, value).</summary>
public sealed record FacetCounts(int TotalCards, IReadOnlyDictionary<(AttributeId Attribute, AttributeValueId Value), int> ByValue);
