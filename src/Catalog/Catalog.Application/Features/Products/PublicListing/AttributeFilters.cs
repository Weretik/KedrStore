using Catalog.Application.Contracts.Persistence;
using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Products.PublicListing;

/// <summary>One attribute of a public filter with the chosen values (any of them matches).</summary>
public sealed record AttributeFilter(AttributeId AttributeId, string AttributeSlug, IReadOnlyList<AttributeValueId> ValueIds);

/// <summary>
/// Reads <c>filter=attribute-slug:value-slug[,value-slug]</c> (spec catalog/006, phase 5) and applies it: values of one
/// attribute match any of them, different attributes all. Only values a person confirmed match (R-06).
/// </summary>
public sealed class AttributeFilters(IReadCatalogDbContext db)
{
    public const int MaxValues = 50;

    /// <summary>Attribute slug → value slugs, or <c>null</c> when an entry is not <c>attribute:value</c>.</summary>
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

    /// <summary>The filters by ID, or <c>null</c> when an attribute or value slug is unknown.</summary>
    public async Task<List<AttributeFilter>?> ResolveAsync(Dictionary<string, SortedSet<string>> parsed,
        CancellationToken cancellationToken)
    {
        if (parsed.Count == 0)
            return [];

        var slugs = parsed.Keys.ToList();
        var attributes = await db.Attributes.AsNoTracking()
            .Where(attribute => slugs.Contains(attribute.Slug))
            .Select(attribute => new { attribute.Id, attribute.Slug })
            .ToListAsync(cancellationToken);
        if (attributes.Count != slugs.Count)
            return null;

        var attributeIds = attributes.Select(attribute => attribute.Id).ToList();
        var values = await db.AttributeValues.AsNoTracking()
            .Where(value => attributeIds.Contains(value.AttributeId))
            .Select(value => new { value.Id, value.AttributeId, value.Slug })
            .ToListAsync(cancellationToken);

        var filters = new List<AttributeFilter>(attributes.Count);
        foreach (var attribute in attributes.OrderBy(attribute => attribute.Slug, StringComparer.Ordinal))
        {
            var bySlug = values.Where(value => value.AttributeId == attribute.Id)
                .ToDictionary(value => value.Slug, value => value.Id, StringComparer.Ordinal);
            var ids = new List<AttributeValueId>();
            foreach (var slug in parsed[attribute.Slug])
            {
                if (!bySlug.TryGetValue(slug, out var id))
                    return null;
                ids.Add(id);
            }

            filters.Add(new AttributeFilter(attribute.Id, attribute.Slug, ids));
        }

        return filters;
    }

    /// <summary>Keeps the products that have a confirmed chosen value of every filtered attribute.</summary>
    public IQueryable<ProductListProjection> Apply(IQueryable<ProductListProjection> query,
        IEnumerable<AttributeFilter> filters)
    {
        foreach (var filter in filters)
        {
            var attributeId = filter.AttributeId;
            var valueIds = filter.ValueIds.Select(id => (AttributeValueId?)id).ToList();
            query = query.Where(projection => db.ProductAttributeValues.Any(value =>
                value.ProductId == projection.ProductId && value.AttributeId == attributeId && value.IsConfirmed
                && valueIds.Contains(value.ValueId)));
        }

        return query;
    }
}
