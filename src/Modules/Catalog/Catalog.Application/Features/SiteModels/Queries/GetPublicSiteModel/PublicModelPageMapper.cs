using Catalog.Contracts.SiteModels.Public;
using Catalog.Application.Features.SiteModels.Shared.Public;
namespace Catalog.Application.Features.SiteModels.Queries.GetPublicSiteModel;

/// <summary>Builds the model page from <see cref="PublicModelCatalog"/>.</summary>
public static class PublicModelPageMapper
{
    public static PublicSiteModelDto ToPage(PublicModelCatalog catalog, PublicModelFacts model, string language)
    {
        var category = catalog.Categories[model.CategoryId];
        var axes = model.Axes.Where(catalog.Attributes.ContainsKey)
            .Select(axis => ToAxis(catalog, model, catalog.Attributes[axis], language))
            .Where(axis => axis.Values.Count > 0)
            .ToList();
        var products = model.Products.Select(id => catalog.Products[id])
            .OrderByDescending(product => product.InStock)
            .ThenBy(product => product.Name(language), StringComparer.CurrentCulture)
            .Select(product => new PublicSiteModelProductDto(product.Id.Value, product.Name(language), product.Slug,
                product.Photo, product.Price, product.InStock, product.IsSale, product.IsNew,
                model.MainProductId == product.Id, catalog.SelectionOf(model, product.Id)))
            .ToList();

        return new PublicSiteModelDto(model.Slug, catalog.NameOf(model, language), model.IsAccepted,
            ToCategory(category, language),
            catalog.PathOf(model.CategoryId).Select(item => ToCategory(item, language)).ToList(),
            TextsOf(catalog, model, language),
            model.MainProductId is { } mainId && model.Products.Contains(mainId) ? catalog.Products[mainId].Slug : null,
            axes, products, Characteristics(catalog, model, language));
    }

    private static PublicSiteModelCategoryDto ToCategory(PublicCategoryFacts category, string language)
        => new(category.Id.Value, category.Slug, category.Name(language));

    /// <summary>The axis values the model's products have, by dictionary order.</summary>
    private static PublicSiteModelAxisDto ToAxis(PublicModelCatalog catalog, PublicModelFacts model,
        PublicAttributeFacts attribute, string language)
    {
        var values = new Dictionary<string, (int Order, PublicSiteModelAxisValueDto Value)>(StringComparer.Ordinal);
        foreach (var productId in model.Products)
        {
            if (catalog.KeyOf(productId, attribute.Id) is not { } key || values.ContainsKey(key))
                continue;

            var first = catalog.DictionaryValuesOf(productId, attribute.Id).FirstOrDefault();
            values[key] = (first?.SortOrder ?? 0, new PublicSiteModelAxisValueDto(key,
                string.Join(" / ", catalog.NamesOf(productId, attribute, language)), first?.Hex, first?.TextureUrl));
        }

        var display = PublicModelCatalog.DisplayOf(attribute.Code) == "swatch" ? "swatch" : "button";
        return new PublicSiteModelAxisDto(attribute.Code, attribute.Slug, attribute.Name(language), display,
            values.Values.OrderBy(item => item.Order).ThenBy(item => item.Value.Key, StringComparer.Ordinal)
                .Select(item => item.Value).ToList());
    }

    /// <summary>Texts of the language; an empty Russian field takes the Ukrainian one.</summary>
    private static PublicSiteModelTextDto TextsOf(PublicModelCatalog catalog, PublicModelFacts model, string language)
    {
        if (model.ModelId is not { } modelId)
            return new PublicSiteModelTextDto(string.Empty, string.Empty, string.Empty, string.Empty);

        var texts = catalog.Texts[modelId].ToList();
        var uk = texts.FirstOrDefault(text => text.Language == "uk");
        var own = texts.FirstOrDefault(text => text.Language == language);
        string Pick(Func<PublicModelTextFacts, string> field)
            => own is not null && field(own).Length > 0 ? field(own) : uk is not null ? field(uk) : string.Empty;

        return new PublicSiteModelTextDto(Pick(text => text.ShortDescription), Pick(text => text.Description),
            Pick(text => text.SeoTitle), Pick(text => text.SeoDescription));
    }

    /// <summary>Attributes outside the axes that every on-site product of the model has with the same values.</summary>
    private static List<PublicSiteModelCharacteristicDto> Characteristics(PublicModelCatalog catalog,
        PublicModelFacts model, string language)
    {
        var first = model.Products[0];
        var axes = model.Axes.ToHashSet();
        return catalog.Values[first].Select(value => value.AttributeId).Distinct()
            .Where(attributeId => !axes.Contains(attributeId) && catalog.Attributes.ContainsKey(attributeId))
            .Where(attributeId => model.Products.Skip(1)
                .All(id => catalog.KeyOf(id, attributeId) == catalog.KeyOf(first, attributeId)))
            .Select(attributeId => catalog.Attributes[attributeId])
            .OrderBy(attribute => attribute.SortOrder).ThenBy(attribute => attribute.Code, StringComparer.Ordinal)
            .Select(attribute => new PublicSiteModelCharacteristicDto(attribute.Code, attribute.Name(language),
                catalog.NamesOf(first, attribute, language)))
            .Where(item => item.Values.Count > 0)
            .ToList();
    }
}
