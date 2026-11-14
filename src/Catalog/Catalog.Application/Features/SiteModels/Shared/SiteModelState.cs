using Catalog.Application.Features.Attributes.Shared.Import;
using Catalog.Contracts.SiteModels;
using Catalog.Domain.Groups;
using Catalog.Domain.Attributes;

namespace Catalog.Application.Features.SiteModels.Shared;

/// <summary>
/// Everything the model pages and the health counts need, read once (the catalog has a few thousand products):
/// products, models, their axes and texts, and the confirmed attribute values. Problems follow spec catalog/007 R-08
/// and count only on-site, not deleted products.
/// </summary>
public sealed class SiteModelState
{
    public required IReadOnlyDictionary<ProductId, SiteModelProductFacts> Products { get; init; }
    public required IReadOnlyDictionary<ProductGroupId, ProductGroup> Models { get; init; }
    public required ILookup<ProductGroupId, ProductId> MembersByModel { get; init; }
    public required IReadOnlyDictionary<ProductId, ProductGroupId> ModelOfProduct { get; init; }
    public required ILookup<ProductGroupId, ProductGroupAxis> SavedAxes { get; init; }
    public required ILookup<ProductGroupId, ProductGroupText> Texts { get; init; }
    public required ILookup<ProductId, SiteModelValueFacts> Values { get; init; }
    public required IReadOnlyDictionary<AttributeId, SiteModelAttributeFacts> Attributes { get; init; }
    public required IReadOnlyDictionary<AttributeValueId, string> ValueNames { get; init; }
    public required IReadOnlyDictionary<ProductCategoryId, string> CategoryNames { get; init; }

    /// <summary>Parent of every active category (null for a top category).</summary>
    public required IReadOnlyDictionary<ProductCategoryId, ProductCategoryId?> CategoryParents { get; init; }

    /// <summary>Active categories marked «show on site» (R-18).</summary>
    public required IReadOnlySet<ProductCategoryId> ShownCategories { get; init; }

    /// <summary>How many attributes each category offers as site filters (its own rows, not its subcategories').</summary>
    public required IReadOnlyDictionary<ProductCategoryId, int> FilterCounts { get; init; }

    /// <summary>Whether a category is the selected one or inside it; no selection matches everything.</summary>
    public bool InCategory(ProductCategoryId categoryId, int? selected)
    {
        if (selected is not { } selectedId)
            return true;

        var visited = new HashSet<ProductCategoryId>();
        for (ProductCategoryId? current = categoryId; current is { } id && visited.Add(id);
             current = CategoryParents.GetValueOrDefault(id))
        {
            if (id.Value == selectedId)
                return true;
        }

        return false;
    }

    /// <summary>An on-site product that is in no model at all, proposed or accepted (R-08).</summary>
    public bool IsLone(ProductId id) => IsOnSite(id) && !ModelOfProduct.ContainsKey(id);

    public bool IsOnSite(ProductId id) => Products.TryGetValue(id, out var product) && product.IsOnSite;

    public IEnumerable<ProductId> OnSiteMembers(ProductGroupId modelId)
        => MembersByModel[modelId].Where(IsOnSite);

    /// <summary>Saved axes in order; a proposed model without saved axes gets the offered ones (R-05).</summary>
    public IReadOnlyList<(SiteModelAttributeFacts Attribute, bool IsSaved)> AxesOf(ProductGroupId modelId)
    {
        var saved = SavedAxes[modelId].OrderBy(axis => axis.Position)
            .Where(axis => Attributes.ContainsKey(axis.AttributeId))
            .Select(axis => (Attributes[axis.AttributeId], true))
            .ToList();
        if (saved.Count > 0 || Models[modelId].IsAccepted)
            return saved;

        return OfferedAxes(modelId).Select(attribute => (attribute, false)).ToList();
    }

    /// <summary>Variant attributes whose confirmed values differ between the model's products (R-05).</summary>
    public IReadOnlyList<SiteModelAttributeFacts> OfferedAxes(ProductGroupId modelId)
    {
        var members = MembersByModel[modelId].Where(id => Products.TryGetValue(id, out var p) && !p.IsDeleted).ToList();
        if (members.Count < 2)
            return [];

        return Attributes.Values.Where(attribute => attribute.IsVariantAxis)
            .Where(attribute => members.Select(id => Signature(id, attribute.Id)).Distinct().Count() > 1)
            .OrderBy(attribute => attribute.SortOrder).ThenBy(attribute => attribute.Code, StringComparer.Ordinal)
            .ToList();
    }

    /// <summary>The confirmed values of one attribute on a product, in position order, as one comparable key.</summary>
    public string Signature(ProductId productId, AttributeId attributeId)
        => string.Join(',', Values[productId].Where(value => value.AttributeId == attributeId)
            .OrderBy(value => value.Position).Select(value => value.ValueId?.Value.ToString() ?? value.Number));

    public IReadOnlyList<string> ProductProblems(ProductId productId)
    {
        if (!IsOnSite(productId))
            return [];
        if (!ModelOfProduct.TryGetValue(productId, out var modelId))
            return [SiteModelProblems.NoModel];

        var problems = new List<string>();
        if (!Values[productId].Any())
            problems.Add(SiteModelProblems.NoAttributes);

        // A proposed model is checked against its offered axes: the person sees what accepting it would leave.
        var axes = AxesOf(modelId).Select(axis => axis.Attribute.Id).ToList();
        if (axes.Count == 0)
            return problems;

        if (axes.Any(axis => Signature(productId, axis).Length == 0))
            problems.Add(SiteModelProblems.MissingAxisValue);
        else
        {
            var key = Combination(productId, axes);
            if (OnSiteMembers(modelId).Any(other => other != productId && Combination(other, axes) == key))
                problems.Add(SiteModelProblems.SameCombination);
        }

        return problems;
    }

    /// <summary>Problems of an accepted model with on-site products, then those of its on-site products.</summary>
    public IReadOnlyList<string> ModelProblems(ProductGroupId modelId, bool withProducts = true)
    {
        var model = Models[modelId];
        var onSite = OnSiteMembers(modelId).ToList();
        var problems = new List<string>();
        if (model.IsAccepted && onSite.Count > 0)
        {
            if (onSite.Count > 1 && !SavedAxes[modelId].Any())
                problems.Add(SiteModelProblems.ModelNoAxes);
            var texts = Texts[modelId].ToDictionary(text => text.Language, StringComparer.Ordinal);
            if (ProductGroupText.Languages.Any(language => !texts.TryGetValue(language, out var text) || !text.IsComplete))
                problems.Add(SiteModelProblems.ModelNoSeo);
            if (onSite.All(id => !Products[id].HasPhoto))
                problems.Add(SiteModelProblems.ModelNoPhoto);
        }

        if (withProducts)
            problems.AddRange(onSite.SelectMany(ProductProblems));

        return problems.Distinct(StringComparer.Ordinal).ToList();
    }

    public SiteModelHealthDto Health()
    {
        var productProblems = Products.Values.Where(product => product.IsOnSite)
            .Select(product => ProductProblems(product.Id)).ToList();
        int CountProducts(string code) => productProblems.Count(problems => problems.Contains(code));

        var accepted = Models.Values.Where(model => model.IsAccepted)
            .Select(model => ModelProblems(model.Id, withProducts: false)).ToList();
        int CountModels(string code) => accepted.Count(problems => problems.Contains(code));

        var proposed = Models.Values.Count(model => !model.IsAccepted && OnSiteMembers(model.Id).Any());
        return new SiteModelHealthDto(
            CountProducts(SiteModelProblems.NoModel), CountProducts(SiteModelProblems.NoAttributes),
            CountProducts(SiteModelProblems.MissingAxisValue), CountProducts(SiteModelProblems.SameCombination),
            CountModels(SiteModelProblems.ModelNoAxes), CountModels(SiteModelProblems.ModelNoSeo),
            CountModels(SiteModelProblems.ModelNoPhoto), proposed);
    }

    public static string StatusText(ProductGroupStatus status)
        => status == ProductGroupStatus.Accepted ? "accepted" : "proposed";

    public static string DisplayOf(SiteModelAttributeFacts attribute)
        => attribute.Code == AttributeImportCatalog.Color ? "swatch" : attribute.IsVariantAxis ? "button" : "checkbox";

    private string Combination(ProductId productId, IReadOnlyList<AttributeId> axes)
        => string.Join('|', axes.Select(axis => Signature(productId, axis)));
}

public sealed record SiteModelProductFacts(
    ProductId Id,
    string Name,
    string Slug,
    ProductCategoryId CategoryId,
    string Photo,
    decimal? RetailPrice,
    bool InStock,
    bool ExportToSite,
    bool IsDeleted,
    bool IsCategoryShown)
{
    /// <summary>Exported from 1C, not deleted, and its category is shown on the site (R-08, R-18).</summary>
    public bool IsOnSite => ExportToSite && !IsDeleted && IsCategoryShown;
    public bool HasPhoto => Photo.Length > 0;
}

/// <summary>A confirmed attribute value of a product; <c>Number</c> holds a number attribute's value as text.</summary>
public sealed record SiteModelValueFacts(
    AttributeId AttributeId, short Position, AttributeValueId? ValueId, string Number, bool IsManual);

public sealed record SiteModelAttributeFacts(
    AttributeId Id, string Code, string Name, bool IsVariantAxis, int SortOrder, AttributeKind Kind);
