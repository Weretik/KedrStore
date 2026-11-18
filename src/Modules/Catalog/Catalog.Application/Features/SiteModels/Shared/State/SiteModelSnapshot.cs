using System.ComponentModel;
using Catalog.Domain.Attributes.Enums;
using Catalog.Domain.Attributes.ValueObjects;
using Catalog.Domain.Groups;

namespace Catalog.Application.Features.SiteModels.Shared.State;

/// <summary>
/// The rows <see cref="SiteModelState"/> is built from, as plain values so the read cache can serialize them (ADR-0016)
/// and keep one shared instance. <see cref="ToState"/> indexes them for one request; that costs memory work only.
/// </summary>
[ImmutableObject(true)]
public sealed record SiteModelSnapshot(
    IReadOnlyList<SiteModelSnapshot.CategoryRow> Categories,
    IReadOnlyList<SiteModelSnapshot.ProductRow> Products,
    IReadOnlyList<SiteModelSnapshot.ModelRow> Models,
    IReadOnlyList<SiteModelSnapshot.MemberRow> Members,
    IReadOnlyList<SiteModelSnapshot.AxisRow> Axes,
    IReadOnlyList<SiteModelSnapshot.TextRow> Texts,
    IReadOnlyList<SiteModelSnapshot.ValueRow> Values,
    IReadOnlyList<SiteModelSnapshot.AttributeRow> Attributes,
    IReadOnlyList<SiteModelSnapshot.FilterCountRow> FilterCounts,
    IReadOnlyList<SiteModelSnapshot.ValueNameRow> ValueNames)
{
    public SiteModelState ToState()
    {
        var shown = Categories.Where(category => category.IsShownOnSite)
            .Select(category => ProductCategoryId.FromStorage(category.Id)).ToHashSet();
        var models = Models.ToDictionary(model => ProductGroupId.FromStorage(model.Id), model => new SiteModelFacts(
            ProductGroupId.FromStorage(model.Id), ProductCategoryId.FromStorage(model.CategoryId), model.Name, model.Slug,
            model.Status, model.AcceptedAt, model.AcceptedBy,
            model.MainProductId is { } main ? ProductId.FromStorage(main) : null));
        var members = Members.Select(member => (Product: ProductId.FromStorage(member.ProductId),
                Model: ProductGroupId.FromStorage(member.ModelId)))
            .Where(member => models.ContainsKey(member.Model))
            .ToList();

        return new SiteModelState
        {
            Products = Products.ToDictionary(product => ProductId.FromStorage(product.Id), product =>
                new SiteModelProductFacts(ProductId.FromStorage(product.Id), product.Name, product.Slug,
                    ProductCategoryId.FromStorage(product.CategoryId), product.Photo, product.RetailPrice,
                    product.InStock, product.ExportToSite, product.IsDeleted,
                    shown.Contains(ProductCategoryId.FromStorage(product.CategoryId)))),
            Models = models,
            MembersByModel = members.ToLookup(member => member.Model, member => member.Product),
            ModelOfProduct = members.ToDictionary(member => member.Product, member => member.Model),
            SavedAxes = Axes.Select(axis => new SiteModelAxisFacts(ProductGroupId.FromStorage(axis.ModelId),
                    AttributeId.FromStorage(axis.AttributeId), axis.Position))
                .ToLookup(axis => axis.GroupId),
            Texts = Texts.Select(text => new SiteModelTextFacts(ProductGroupId.FromStorage(text.ModelId), text.Language,
                    text.ShortDescription, text.Description, text.SeoTitle, text.SeoDescription, text.Version,
                    text.IsComplete))
                .ToLookup(text => text.GroupId),
            Values = Values.ToLookup(value => ProductId.FromStorage(value.ProductId), value => new SiteModelValueFacts(
                AttributeId.FromStorage(value.AttributeId), value.Position,
                value.ValueId is { } valueId ? AttributeValueId.FromStorage(valueId) : null, value.Number,
                value.IsManual)),
            Attributes = Attributes.ToDictionary(attribute => AttributeId.FromStorage(attribute.Id), attribute =>
                new SiteModelAttributeFacts(AttributeId.FromStorage(attribute.Id), attribute.Code, attribute.Name,
                    attribute.IsVariantAxis, attribute.SortOrder, attribute.Kind)),
            ValueNames = ValueNames.ToDictionary(value => AttributeValueId.FromStorage(value.Id), value => value.Name),
            CategoryNames = Categories.ToDictionary(category => ProductCategoryId.FromStorage(category.Id),
                category => category.Name),
            CategoryParents = Categories.ToDictionary(category => ProductCategoryId.FromStorage(category.Id),
                category => category.ParentId is { } parent ? ProductCategoryId.FromStorage(parent) : (ProductCategoryId?)null),
            ShownCategories = shown,
            FilterCounts = FilterCounts.ToDictionary(row => ProductCategoryId.FromStorage(row.CategoryId), row => row.Count)
        };
    }

    /// <summary>An active category; <c>Name</c> is its short name, or its name when it has none.</summary>
    public sealed record CategoryRow(int Id, int? ParentId, string Name, bool IsShownOnSite);

    public sealed record ProductRow(int Id, string Name, string Slug, int CategoryId, string Photo, decimal? RetailPrice,
        bool InStock, bool ExportToSite, bool IsDeleted);

    public sealed record ModelRow(int Id, int CategoryId, string Name, string Slug, ProductGroupStatus Status,
        DateTimeOffset? AcceptedAt, string? AcceptedBy, int? MainProductId);

    public sealed record MemberRow(int ProductId, int ModelId);

    public sealed record AxisRow(int ModelId, int AttributeId, short Position);

    /// <summary><c>IsComplete</c> is the domain rule of the model text, taken when the row is read.</summary>
    public sealed record TextRow(int ModelId, string Language, string ShortDescription, string Description,
        string SeoTitle, string SeoDescription, uint Version, bool IsComplete);

    /// <summary>A confirmed value; <c>Number</c> holds a number attribute's value as text.</summary>
    public sealed record ValueRow(int ProductId, int AttributeId, short Position, int? ValueId, string Number,
        bool IsManual);

    public sealed record AttributeRow(int Id, string Code, string Name, bool IsVariantAxis, int SortOrder,
        AttributeKind Kind);

    public sealed record FilterCountRow(int CategoryId, int Count);

    public sealed record ValueNameRow(int Id, string Name);
}
