using Catalog.Application.Features.SiteModels.Shared.Public;
using Catalog.Contracts.SiteModels.Public;
using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;

namespace Catalog.Application.Features.SiteModels.Queries.GetPublicSiteModels;

/// <summary>
/// What a buyer chose in a model list: attribute values (one attribute any, different attributes all; confirmed values
/// only) and the common filters. A model matches when one of its on-site products matches everything.
/// </summary>
public sealed class PublicModelFilter
{
    public const int MaxValues = 50;

    private readonly GetPublicSiteModelsRequest request;

    private PublicModelFilter(GetPublicSiteModelsRequest request,
        IReadOnlyDictionary<AttributeId, HashSet<AttributeValueId>> chosen)
    {
        this.request = request;
        Chosen = chosen;
    }

    /// <summary>Chosen values by attribute.</summary>
    public IReadOnlyDictionary<AttributeId, HashSet<AttributeValueId>> Chosen { get; }

    /// <summary>Attribute slug → value slugs, or <c>null</c> when an entry is not <c>attribute:value</c> or too long.</summary>
    public static Dictionary<string, SortedSet<string>>? Parse(IEnumerable<string>? entries)
    {
        var result = new Dictionary<string, SortedSet<string>>(StringComparer.Ordinal);
        foreach (var entry in entries ?? [])
        {
            if (string.IsNullOrWhiteSpace(entry))
                continue;

            var separator = entry.IndexOf(':');
            if (separator <= 0)
                return null;

            var attribute = entry[..separator].Trim().ToLowerInvariant();
            var values = entry[(separator + 1)..]
                .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(value => value.ToLowerInvariant())
                .ToList();
            if (attribute.Length == 0 || values.Count == 0)
                return null;

            if (!result.TryGetValue(attribute, out var set))
                result[attribute] = set = new SortedSet<string>(StringComparer.Ordinal);
            set.UnionWith(values);
        }

        return result.Values.Sum(set => set.Count) > MaxValues ? null : result;
    }

    /// <summary>The filter by IDs, or <c>null</c> when an attribute or value slug is unknown.</summary>
    public static PublicModelFilter? Resolve(GetPublicSiteModelsRequest request,
        IReadOnlyDictionary<string, SortedSet<string>> parsed, PublicModelCatalog catalog)
    {
        var chosen = new Dictionary<AttributeId, HashSet<AttributeValueId>>();
        foreach (var (attributeSlug, valueSlugs) in parsed)
        {
            var attribute = catalog.Attributes.Values.FirstOrDefault(item => item.Slug == attributeSlug);
            if (attribute is null)
                return null;

            var bySlug = catalog.Dictionary.Values.Where(value => value.AttributeId == attribute.Id)
                .ToDictionary(value => value.Slug, value => value.Id, StringComparer.Ordinal);
            var ids = new HashSet<AttributeValueId>();
            foreach (var slug in valueSlugs)
            {
                if (!bySlug.TryGetValue(slug, out var id))
                    return null;
                ids.Add(id);
            }

            chosen[attribute.Id] = ids;
        }

        return new PublicModelFilter(request, chosen);
    }

    /// <summary>Stock, sale, new, and price.</summary>
    public bool MatchesCommon(PublicProductFacts product)
        => (request.InStock != true || product.InStock)
           && (request.IsSale is not { } isSale || product.IsSale == isSale)
           && (request.IsNew is not { } isNew || product.IsNew == isNew)
           && (request.PriceFrom is not { } from || product.Price >= from)
           && (request.PriceTo is not { } to || product.Price <= to);

    /// <summary>Chosen attributes the product has none of the chosen values of (at most <paramref name="limit"/>).</summary>
    public List<AttributeId> Missing(PublicModelCatalog catalog, ProductId productId, int limit = int.MaxValue)
        => Chosen.Where(pair => !catalog.Values[productId]
                .Any(value => value.AttributeId == pair.Key && value.ValueId is { } id && pair.Value.Contains(id)))
            .Select(pair => pair.Key)
            .Take(limit)
            .ToList();

    public bool Matches(PublicModelCatalog catalog, PublicProductFacts product)
        => MatchesCommon(product) && Missing(catalog, product.Id, 1).Count == 0;
}
