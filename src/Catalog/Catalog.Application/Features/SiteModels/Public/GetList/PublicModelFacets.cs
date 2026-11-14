using Catalog.Application.Features.SiteModels.Public.Common;
using Catalog.Contracts.SiteModels.Public;
using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;

namespace Catalog.Application.Features.SiteModels.Public.GetList;

/// <summary>
/// Attribute filters of a final category with model counts (M-09). A value's count applies the common filters and
/// the choices of every other attribute but not its own, so a buyer who chose «Хром» sees how many models adding
/// «Чорний» gives. A value without models is left out unless chosen.
/// </summary>
public static class PublicModelFacets
{
    public static IReadOnlyList<PublicSiteModelFilterDto> Count(PublicModelCatalog catalog,
        ProductCategoryId categoryId, IReadOnlyList<PublicModelFacts> models, PublicModelFilter filter,
        string language)
    {
        var attributes = catalog.CategoryFilters[categoryId]
            .Where(row => catalog.Attributes.ContainsKey(row.AttributeId))
            .OrderBy(row => row.SortOrder).ThenBy(row => catalog.Attributes[row.AttributeId].Code, StringComparer.Ordinal)
            .Select(row => catalog.Attributes[row.AttributeId])
            .ToList();
        if (attributes.Count == 0)
            return [];

        var counted = attributes.Select(attribute => attribute.Id).ToHashSet();
        var byValue = new Dictionary<AttributeValueId, HashSet<string>>();
        foreach (var model in models)
        {
            foreach (var productId in model.Products)
            {
                if (!filter.MatchesCommon(catalog.Products[productId]))
                    continue;

                // A product counts for attribute A when every other attribute's choice matches.
                var missing = filter.Missing(catalog, productId, 2);
                if (missing.Count > 1)
                    continue;

                foreach (var value in catalog.Values[productId])
                {
                    if (value.ValueId is not { } valueId || !counted.Contains(value.AttributeId)
                        || (missing.Count == 1 && missing[0] != value.AttributeId))
                        continue;

                    if (!byValue.TryGetValue(valueId, out var slugs))
                        byValue[valueId] = slugs = new HashSet<string>(StringComparer.Ordinal);
                    slugs.Add(model.Slug);
                }
            }
        }

        var chosen = filter.Chosen.Values.SelectMany(ids => ids).ToHashSet();
        return attributes.Select(attribute => new PublicSiteModelFilterDto(attribute.Code, attribute.Slug,
                attribute.Name(language), PublicModelCatalog.DisplayOf(attribute.Code),
                catalog.Dictionary.Values.Where(value => value.AttributeId == attribute.Id)
                    .Select(value => (Value: value, Count: byValue.GetValueOrDefault(value.Id)?.Count ?? 0))
                    .Where(item => item.Count > 0 || chosen.Contains(item.Value.Id))
                    .OrderBy(item => item.Value.SortOrder).ThenBy(item => item.Value.NameUk, StringComparer.Ordinal)
                    .Select(item => new PublicSiteModelFilterValueDto(item.Value.Slug, item.Value.Name(language),
                        item.Value.Hex, item.Value.TextureUrl, item.Count, chosen.Contains(item.Value.Id)))
                    .ToList()))
            .Where(facet => facet.Values.Count > 0)
            .ToList();
    }
}
