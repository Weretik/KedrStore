using Catalog.Application.Attributes.Import;
using Catalog.Application.Features.SiteModels.Public.Common;
using Catalog.Application.Querying;
using Catalog.Application.Slugs;
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
/// Model cards (spec catalog/007 stage 5): a category shows the models of its whole tree; attribute filters and their
/// counts only in a final category (M-09); the search finds models by name, 1C code, and product names (M-12). A former
/// category slug still selects its category.
/// </summary>
public sealed class GetPublicSiteModelsQueryHandler(PublicModelCatalogReader reader, ISlugService slugService)
    : IQueryHandler<GetPublicSiteModelsQuery, Result<PublicSiteModelListDto>>
{
    public async ValueTask<Result<PublicSiteModelListDto>> Handle(GetPublicSiteModelsQuery query,
        CancellationToken cancellationToken)
    {
        var request = query.Request;
        var language = CatalogLanguage.Normalize(query.Lang);
        var catalog = await reader.ReadAsync(cancellationToken);

        ProductCategoryId? categoryId = null;
        if (!string.IsNullOrWhiteSpace(query.CategorySlug))
        {
            var id = await slugService.ResolveAsync(CatalogEntityType.Category,
                query.CategorySlug.Trim().ToLowerInvariant(), cancellationToken);
            if (id is null || !catalog.Categories.ContainsKey(ProductCategoryId.From(id.Value)))
                return Result.NotFound();
            categoryId = ProductCategoryId.From(id.Value);
        }

        var isFinal = categoryId is { } selected && catalog.IsFinal(selected);
        var parsed = PublicModelFilter.Parse(request.Filter)!;
        if (parsed.Count > 0 && !isFinal)
            return Result.Invalid(new ValidationError("filter", "Attribute filters work in a final category only.",
                "Catalog.SiteModels.FilterNotFinal", ValidationSeverity.Error));
        var filter = PublicModelFilter.Resolve(request, parsed, catalog);
        if (filter is null)
            return Result.NotFound();

        var tree = categoryId is { } root ? catalog.TreeOf(root) : null;
        var tokens = Tokens(request.Search);
        var inScope = catalog.Models
            .Where(model => tree is null || tree.Contains(model.CategoryId))
            .Where(model => tokens.Count == 0 || MatchesSearch(catalog, model, tokens))
            .ToList();

        var cards = inScope
            .Select(model => (Model: model, Products: model.Products
                .Select(id => catalog.Products[id])
                .Where(product => filter.Matches(catalog, product))
                .ToList()))
            .Where(item => item.Products.Count > 0)
            .Select(item => ToCard(catalog, item.Model, item.Products, language))
            .ToList();

        var sorted = Sort(cards, request.Sort).ToList();
        var page = request.Page;
        var pageSize = request.PageSize;
        var totalPages = (int)Math.Ceiling(sorted.Count / (double)pageSize);
        var items = sorted.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        var filters = isFinal ? PublicModelFacets.Count(catalog, categoryId!.Value, inScope, filter, language) : [];
        return Result.Success(new PublicSiteModelListDto(
            categoryId is { } current ? catalog.Categories[current].Slug : null, isFinal, page, pageSize, totalPages,
            sorted.Count, items, filters, ExactMatch(catalog, request.Search)));
    }

    private static PublicSiteModelCardDto ToCard(PublicModelCatalog catalog, PublicModelFacts model,
        IReadOnlyList<PublicProductFacts> matching, string language)
    {
        var colorAxis = model.Axes.Cast<AttributeId?>().FirstOrDefault(axis =>
            catalog.Attributes.TryGetValue(axis!.Value, out var attribute) && attribute.Code == AttributeImportCatalog.Color);
        var colors = model.Products.Count > 1 && colorAxis is { } color
            ? model.Products.SelectMany(id => catalog.DictionaryValuesOf(id, color))
                .DistinctBy(value => value.Id)
                .OrderBy(value => value.SortOrder).ThenBy(value => value.Slug, StringComparer.Ordinal)
                .Select(value => new PublicSiteModelSwatchDto(value.Slug, value.Name(language), value.Hex, value.TextureUrl))
                .ToList()
            : [];

        return new PublicSiteModelCardDto(model.Slug, catalog.NameOf(model, language),
            catalog.Categories[model.CategoryId].Slug, catalog.PhotoOf(model),
            matching.Select(product => product.Price).Where(price => price is not null).Min(),
            matching.Any(product => product.InStock), matching.Any(product => product.IsSale),
            matching.Any(product => product.IsNew), model.Products.Count, model.IsAccepted, colors,
            model.Products.Count == 1 ? model.Products[0].Value : null);
    }

    private static IEnumerable<PublicSiteModelCardDto> Sort(IEnumerable<PublicSiteModelCardDto> cards, string? sort)
        => sort switch
        {
            "price_asc" => cards.OrderBy(card => card.PriceFrom is null).ThenBy(card => card.PriceFrom)
                .ThenBy(card => card.Name, StringComparer.CurrentCulture),
            "price_desc" => cards.OrderBy(card => card.PriceFrom is null).ThenByDescending(card => card.PriceFrom)
                .ThenBy(card => card.Name, StringComparer.CurrentCulture),
            "name_asc" => cards.OrderBy(card => card.Name, StringComparer.CurrentCulture),
            "new" => cards.OrderByDescending(card => card.IsNew).ThenByDescending(card => card.InStock)
                .ThenBy(card => card.Name, StringComparer.CurrentCulture),
            _ => cards.OrderByDescending(card => card.InStock).ThenBy(card => card.Name, StringComparer.CurrentCulture)
        };

    private static List<string> Tokens(string? search)
        => (search ?? string.Empty)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(token => token.ToLowerInvariant())
            .ToList();

    /// <summary>Every word is in the model name, or in one product's names or 1C code (M-12).</summary>
    private static bool MatchesSearch(PublicModelCatalog catalog, PublicModelFacts model, List<string> tokens)
    {
        bool All(string text) => tokens.All(token => text.Contains(token, StringComparison.OrdinalIgnoreCase));

        return All(model.Name) || model.Products.Select(id => catalog.Products[id])
            .Any(product => All($"{product.Id.Value} {product.NameUk} {product.NameRu}"));
    }

    /// <summary>The model address of the on-site product whose 1C code is the whole search.</summary>
    private static PublicSiteModelAddressDto? ExactMatch(PublicModelCatalog catalog, string? search)
        => int.TryParse(search?.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var code) && code is > 0 and <= 1_000_000_000
            ? catalog.AddressOf(ProductId.From(code))
            : null;
}
