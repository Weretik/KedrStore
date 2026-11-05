using Catalog.Application.Attributes.Import;
using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.Products.PublicListing;
using Catalog.Application.Querying;
using Catalog.Contracts.Products.Facets;
using Catalog.Domain.Enums;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.Products.GetFacets;

/// <summary>
/// Filters of a category tree (spec catalog/006, phase 5): the attributes its «Фільтри категорій» turn on, with the
/// values its on-site products have and card counts. Only confirmed values count (R-06); a value without cards is
/// left out unless chosen.
/// </summary>
public sealed class GetCategoryFacetsQueryHandler(
    IReadCatalogDbContext db,
    PublicProductQuery publicProducts,
    AttributeFilters attributeFilters,
    VariantGroupReader variantGroups)
    : IQueryHandler<GetCategoryFacetsQuery, Result<CategoryFacetsDto>>
{
    public async ValueTask<Result<CategoryFacetsDto>> Handle(GetCategoryFacetsQuery query,
        CancellationToken cancellationToken)
    {
        var request = query.Request;
        var language = CatalogLanguage.Normalize(request.Lang);
        var categoryIds = await publicProducts.SelectedCategoryTreeIdsAsync(request, cancellationToken);
        var products = await publicProducts.BuildAsync(request, language, cancellationToken);
        if (categoryIds is not { Length: > 0 } || products is null)
            return Result.NotFound();

        var parsedFilters = AttributeFilters.Parse(request.Filter);
        if (parsedFilters is null)
            return Result.Invalid(new ValidationError("filter", "Use attribute-slug:value-slug[,value-slug].",
                "Catalog.Products.InvalidFilter", ValidationSeverity.Error));
        var filters = await attributeFilters.ResolveAsync(parsedFilters, cancellationToken);
        if (filters is null)
            return Result.NotFound();

        var facetAttributes = await FacetAttributesAsync(categoryIds, cancellationToken);
        var countedIds = facetAttributes.Select(attribute => attribute.Id).ToHashSet();
        var neededIds = countedIds.Concat(filters.Select(filter => filter.AttributeId)).Distinct().ToList();

        var productIds = await products.Select(projection => projection.ProductId.Value).ToListAsync(cancellationToken);
        var candidates = variantGroups.Candidates(productIds, await variantGroups.MembershipsAsync(cancellationToken));
        var onSite = products.Select(projection => projection.ProductId);
        var values = (await db.ProductAttributeValues.AsNoTracking()
                .Where(value => onSite.Contains(value.ProductId) && value.IsConfirmed && value.ValueId != null
                                && neededIds.Contains(value.AttributeId))
                .Select(value => new { value.ProductId, value.AttributeId, ValueId = value.ValueId!.Value })
                .ToListAsync(cancellationToken))
            .ToLookup(value => value.ProductId.Value, value => (value.AttributeId, value.ValueId));

        var counts = FacetCounter.Count(candidates, values, filters, countedIds);
        var chosen = filters.SelectMany(filter => filter.ValueIds).ToHashSet();
        var dictionary = await db.AttributeValues.AsNoTracking()
            .Where(value => countedIds.Contains(value.AttributeId))
            .ToListAsync(cancellationToken);
        var textures = await variantGroups.TextureUrlsAsync(dictionary.Select(value => value.SwatchMediaFileId),
            cancellationToken);

        var facets = new List<CategoryFacetDto>();
        foreach (var attribute in facetAttributes)
        {
            var facetValues = dictionary
                .Where(value => value.AttributeId == attribute.Id)
                .Select(value => (Value: value, Count: counts.ByValue.GetValueOrDefault((attribute.Id, value.Id))))
                .Where(item => item.Count > 0 || chosen.Contains(item.Value.Id))
                .OrderBy(item => item.Value.SortOrder).ThenBy(item => item.Value.NameUk, StringComparer.Ordinal)
                .Select(item => new CategoryFacetValueDto(item.Value.Slug,
                    Localized(language, item.Value.NameUk, item.Value.NameRu), item.Value.SwatchHex,
                    item.Value.SwatchMediaFileId is { } mediaId ? textures.GetValueOrDefault(mediaId) : null,
                    item.Count, chosen.Contains(item.Value.Id)))
                .ToList();
            if (facetValues.Count > 0)
                facets.Add(new CategoryFacetDto(attribute.Code, attribute.Slug,
                    Localized(language, attribute.NameUk, attribute.NameRu), Display(attribute.Code), facetValues));
        }

        return Result.Success(new CategoryFacetsDto(counts.TotalCards, facets));
    }

    /// <summary>Attributes the tree's categories offer as filters, by their order; number attributes are not offered.</summary>
    private async Task<List<FacetAttribute>> FacetAttributesAsync(ProductCategoryId[] categoryIds,
        CancellationToken cancellationToken)
    {
        var rows = await (from row in db.CategoryAttributes.AsNoTracking()
                where categoryIds.Contains(row.CategoryId) && row.IsFilterable
                join attribute in db.Attributes.AsNoTracking() on row.AttributeId equals attribute.Id
                where attribute.IsFilterable && attribute.Kind != AttributeKind.Number
                select new FacetAttribute(attribute.Id, attribute.Code, attribute.Slug, attribute.NameUk,
                    attribute.NameRu, row.SortOrder))
            .ToListAsync(cancellationToken);
        return rows.GroupBy(row => row.Id)
            .Select(group => group.MinBy(row => row.SortOrder)!)
            .OrderBy(row => row.SortOrder).ThenBy(row => row.Code, StringComparer.Ordinal)
            .ToList();
    }

    private static string Display(string code) => code switch
    {
        AttributeImportCatalog.Color => "swatch",
        AttributeImportCatalog.Size or AttributeImportCatalog.Side => "button",
        _ => "checkbox"
    };

    private static string Localized(string language, string uk, string? ru)
        => language == "ru" && !string.IsNullOrWhiteSpace(ru) ? ru : uk;

    private sealed record FacetAttribute(AttributeId Id, string Code, string Slug, string NameUk, string? NameRu,
        int SortOrder);
}
