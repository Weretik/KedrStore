using Catalog.Domain.Attributes;
using Catalog.Domain.Attributes.Enums;
using Catalog.Domain.Attributes.ValueObjects;
using Catalog.Domain.Groups;

namespace Catalog.Application.Features.SiteModels.Shared.State;

/// <summary>A model as <see cref="SiteModelState"/> keeps it: a read model of <see cref="ProductGroup"/>.</summary>
public sealed record SiteModelFacts(
    ProductGroupId Id,
    ProductCategoryId CategoryId,
    string Name,
    string Slug,
    ProductGroupStatus Status,
    DateTimeOffset? AcceptedAt,
    string? AcceptedBy,
    ProductId? MainProductId)
{
    public bool IsAccepted => Status == ProductGroupStatus.Accepted;
}

/// <summary>A saved axis of a model.</summary>
public sealed record SiteModelAxisFacts(ProductGroupId GroupId, AttributeId AttributeId, short Position);

/// <summary>A model's text in one language; <c>IsComplete</c> is the domain rule of <see cref="ProductGroupText"/>.</summary>
public sealed record SiteModelTextFacts(
    ProductGroupId GroupId,
    string Language,
    string ShortDescription,
    string Description,
    string SeoTitle,
    string SeoDescription,
    uint Version,
    bool IsComplete);

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
