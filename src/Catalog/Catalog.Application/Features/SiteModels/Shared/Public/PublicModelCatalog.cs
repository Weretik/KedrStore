using Catalog.Application.Features.Attributes.Shared.Import;
using Catalog.Contracts.SiteModels.Public;
using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;

namespace Catalog.Application.Features.SiteModels.Shared.Public;

/// <summary>
/// The public site's catalog of models, read once per request (a few thousand on-site products): on-site products of
/// public categories, models (accepted ones and products of their own, M-04), confirmed attribute values, the shared
/// dictionary, categories, their filters, and the model texts.
/// </summary>
public sealed class PublicModelCatalog
{
    /// <summary>Joins the values of one axis when a product has several (for example a two-tone color).</summary>
    public const string KeySeparator = "_";

    public required IReadOnlyDictionary<ProductId, PublicProductFacts> Products { get; init; }
    public required IReadOnlyList<PublicModelFacts> Models { get; init; }
    public required IReadOnlyDictionary<string, PublicModelFacts> ModelBySlug { get; init; }
    public required IReadOnlyDictionary<ProductId, PublicModelFacts> ModelOfProduct { get; init; }
    public required ILookup<ProductId, PublicProductValueFacts> Values { get; init; }
    public required IReadOnlyDictionary<AttributeId, PublicAttributeFacts> Attributes { get; init; }
    public required IReadOnlyDictionary<AttributeValueId, PublicValueFacts> Dictionary { get; init; }
    public required IReadOnlyDictionary<ProductCategoryId, PublicCategoryFacts> Categories { get; init; }
    public required ILookup<ProductCategoryId, PublicCategoryFilterFacts> CategoryFilters { get; init; }
    public required ILookup<ProductGroupId, PublicModelTextFacts> Texts { get; init; }

    private ILookup<ProductCategoryId, ProductCategoryId>? children;

    private ILookup<ProductCategoryId, ProductCategoryId> Children => children ??= Categories.Values
        .Where(category => category.ParentId is not null)
        .ToLookup(category => category.ParentId!.Value, category => category.Id);

    /// <summary><c>swatch</c> (colors), <c>button</c> (size, side), or <c>checkbox</c>.</summary>
    public static string DisplayOf(string attributeCode) => attributeCode switch
    {
        AttributeImportCatalog.Color => "swatch",
        AttributeImportCatalog.Size or AttributeImportCatalog.Side => "button",
        _ => "checkbox"
    };

    /// <summary>A final category has no child categories (M-03, M-09).</summary>
    public bool IsFinal(ProductCategoryId categoryId) => !Children[categoryId].Any();

    /// <summary>The category and every category below it.</summary>
    public HashSet<ProductCategoryId> TreeOf(ProductCategoryId categoryId)
    {
        var result = new HashSet<ProductCategoryId>();
        var pending = new Stack<ProductCategoryId>([categoryId]);
        while (pending.TryPop(out var id))
        {
            if (!result.Add(id))
                continue;
            foreach (var child in Children[id])
                pending.Push(child);
        }

        return result;
    }

    /// <summary>The root first, the category last.</summary>
    public IReadOnlyList<PublicCategoryFacts> PathOf(ProductCategoryId categoryId)
    {
        var path = new List<PublicCategoryFacts>();
        var seen = new HashSet<ProductCategoryId>();
        for (ProductCategoryId? id = categoryId;
             id is { } current && seen.Add(current) && Categories.TryGetValue(current, out var category);
             id = category.ParentId)
            path.Add(category);

        path.Reverse();
        return path;
    }

    /// <summary>
    /// The product's value key of an attribute: the value slugs in position order joined with <see cref="KeySeparator"/>,
    /// or the number as text; <c>null</c> when it has none.
    /// </summary>
    public string? KeyOf(ProductId productId, AttributeId attributeId)
    {
        var parts = Values[productId].Where(value => value.AttributeId == attributeId)
            .OrderBy(value => value.Position)
            .Select(value => value.ValueId is { } valueId
                ? Dictionary.GetValueOrDefault(valueId)?.Slug
                : value.Number?.ToString("0.###", CultureInfo.InvariantCulture))
            .OfType<string>()
            .ToList();
        return parts.Count == 0 ? null : string.Join(KeySeparator, parts);
    }

    /// <summary>The display names of the product's values of an attribute (a number gets its unit).</summary>
    public IReadOnlyList<string> NamesOf(ProductId productId, PublicAttributeFacts attribute, string language)
        => Values[productId].Where(value => value.AttributeId == attribute.Id)
            .OrderBy(value => value.Position)
            .Select(value => value.ValueId is { } valueId
                ? Dictionary.GetValueOrDefault(valueId)?.Name(language)
                : value.Number is { } number
                    ? string.IsNullOrWhiteSpace(attribute.Unit)
                        ? number.ToString("0.###", CultureInfo.InvariantCulture)
                        : $"{number.ToString("0.###", CultureInfo.InvariantCulture)} {attribute.Unit}"
                    : null)
            .OfType<string>()
            .ToList();

    /// <summary>Dictionary values of the product's attribute, in position order.</summary>
    public IReadOnlyList<PublicValueFacts> DictionaryValuesOf(ProductId productId, AttributeId attributeId)
        => Values[productId].Where(value => value.AttributeId == attributeId && value.ValueId is not null)
            .OrderBy(value => value.Position)
            .Select(value => Dictionary.GetValueOrDefault(value.ValueId!.Value))
            .OfType<PublicValueFacts>()
            .ToList();

    /// <summary>Axis slug → value key of the product within its model.</summary>
    public IReadOnlyDictionary<string, string> SelectionOf(PublicModelFacts model, ProductId productId)
    {
        var selection = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var axis in model.Axes)
        {
            if (Attributes.TryGetValue(axis, out var attribute) && KeyOf(productId, axis) is { } key)
                selection[attribute.Slug] = key;
        }

        return selection;
    }

    /// <summary>The model address of an on-site product, or <c>null</c>.</summary>
    public PublicSiteModelAddressDto? AddressOf(ProductId productId)
        => ModelOfProduct.TryGetValue(productId, out var model) && Categories.TryGetValue(model.CategoryId, out var category)
            ? new PublicSiteModelAddressDto(model.Slug, category.Slug, SelectionOf(model, productId))
            : null;

    /// <summary>An accepted model has one name; a product of its own shows the product name of the language.</summary>
    public string NameOf(PublicModelFacts model, string language)
        => model.IsAccepted ? model.Name : Products[model.Products[0]].Name(language);

    /// <summary>The model's photo: its main product's, else the first product photo.</summary>
    public string PhotoOf(PublicModelFacts model)
    {
        if (model.MainProductId is { } mainId && model.Products.Contains(mainId) && Products[mainId].Photo.Length > 0)
            return Products[mainId].Photo;

        return model.Products.Select(id => Products[id].Photo).FirstOrDefault(photo => photo.Length > 0) ?? string.Empty;
    }
}
