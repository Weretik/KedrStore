using Catalog.Domain.Attributes;
using Catalog.Domain.Groups;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;

namespace Catalog.Application.Features.SiteModels.Shared.Public;

/// <summary>An on-site product as the public site shows it.</summary>
public sealed record PublicProductFacts(
    ProductId Id,
    string NameUk,
    string NameRu,
    string Slug,
    ProductCategoryId CategoryId,
    string Photo,
    decimal? Price,
    bool InStock,
    bool IsSale,
    bool IsNew)
{
    public string Name(string language) => language == "ru" && NameRu.Length > 0 ? NameRu : NameUk;
}

/// <summary>
/// A model of the public site: an accepted model with its on-site products, or an on-site product outside accepted
/// models shown as a model of its own under its product slug (M-04).
/// </summary>
public sealed record PublicModelFacts(
    string Slug,
    string Name,
    ProductCategoryId CategoryId,
    ProductGroupId? ModelId,
    ProductId? MainProductId,
    IReadOnlyList<ProductId> Products,
    IReadOnlyList<AttributeId> Axes)
{
    public bool IsAccepted => ModelId is not null;
}

public sealed record PublicAttributeFacts(
    AttributeId Id,
    string Code,
    string Slug,
    string NameUk,
    string? NameRu,
    AttributeKind Kind,
    string? Unit,
    int SortOrder)
{
    public string Name(string language) => PublicText.Localized(language, NameUk, NameRu);
}

public sealed record PublicValueFacts(
    AttributeValueId Id,
    AttributeId AttributeId,
    string Slug,
    string NameUk,
    string? NameRu,
    int SortOrder,
    string? Hex,
    string? TextureUrl)
{
    public string Name(string language) => PublicText.Localized(language, NameUk, NameRu);
}

/// <summary>A confirmed attribute value of an on-site product: a dictionary value or a number.</summary>
public sealed record PublicProductValueFacts(AttributeId AttributeId, short Position, AttributeValueId? ValueId,
    decimal? Number);

public sealed record PublicCategoryFacts(
    ProductCategoryId Id,
    ProductCategoryId? ParentId,
    string Slug,
    string NameUk,
    string NameRu)
{
    public string Name(string language) => PublicText.Localized(language, NameUk, NameRu);
}

/// <summary>An attribute a category offers as a filter, in its order.</summary>
public sealed record PublicCategoryFilterFacts(ProductCategoryId CategoryId, AttributeId AttributeId, int SortOrder);

/// <summary>The four model texts of one language.</summary>
public sealed record PublicModelTextFacts(
    ProductGroupId ModelId,
    string Language,
    string ShortDescription,
    string Description,
    string SeoTitle,
    string SeoDescription);

internal static class PublicText
{
    public static string Localized(string language, string uk, string? ru)
        => language == "ru" && !string.IsNullOrWhiteSpace(ru) ? ru : uk;
}
