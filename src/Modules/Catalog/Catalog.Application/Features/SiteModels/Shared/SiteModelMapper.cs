using Catalog.Contracts.SiteModels;
using Catalog.Domain.Groups;
using Catalog.Domain.Attributes;
using Catalog.Application.Features.SiteModels.Shared.State;
using Catalog.Domain.Attributes.ValueObjects;

namespace Catalog.Application.Features.SiteModels.Shared;

/// <summary>Builds the admin DTOs of a model from <see cref="SiteModelState"/>.</summary>
public static class SiteModelMapper
{
    public static SiteModelListItemDto ToListItem(SiteModelState state, ProductGroupId modelId)
    {
        var model = state.Models[modelId];
        var members = Visible(state, modelId);
        var onSite = members.Where(state.IsOnSite).ToList();
        var photo = PhotoOf(state, model, onSite.Count > 0 ? onSite : members);
        return new SiteModelListItemDto(model.Id.Value, model.CategoryId.Value,
            state.CategoryNames.GetValueOrDefault(model.CategoryId) ?? string.Empty, model.Name, model.Slug,
            SiteModelState.StatusText(model.Status), members.Count, onSite.Count,
            onSite.Select(id => state.Products[id].RetailPrice).Where(price => price is not null).Min(),
            photo, state.AxesOf(modelId).Select(axis => axis.Attribute.Name).ToList(), state.ModelProblems(modelId));
    }

    public static SiteModelDto ToDetail(SiteModelState state, ProductGroupId modelId)
    {
        var model = state.Models[modelId];
        var axes = state.AxesOf(modelId);
        var members = Visible(state, modelId)
            .OrderByDescending(state.IsOnSite)
            .ThenBy(id => state.Products[id].Name, StringComparer.CurrentCulture)
            .ToList();

        var products = members.Select(id =>
        {
            var product = state.Products[id];
            var values = axes.Select(axis =>
            {
                var value = state.Values[id].Where(item => item.AttributeId == axis.Attribute.Id)
                    .OrderBy(item => item.Position).FirstOrDefault();
                var name = value?.ValueId is { } valueId ? state.ValueNames.GetValueOrDefault(valueId) : value?.Number;
                return new SiteModelProductValueDto(axis.Attribute.Id.Value, value?.ValueId?.Value,
                    string.IsNullOrEmpty(name) ? null : name, value?.IsManual ?? false);
            }).ToList();
            return new SiteModelProductDto(id.Value, product.Name, product.Slug, product.Photo, product.RetailPrice,
                product.InStock, product.ExportToSite, model.MainProductId == id, values, state.ProductProblems(id));
        }).ToList();

        var texts = ProductGroupText.Languages.Select(language =>
        {
            var text = state.Texts[modelId].FirstOrDefault(item => item.Language == language);
            return new SiteModelTextDto(language, text?.ShortDescription ?? string.Empty,
                text?.Description ?? string.Empty, text?.SeoTitle ?? string.Empty, text?.SeoDescription ?? string.Empty);
        }).ToList();

        return new SiteModelDto(model.Id.Value, model.CategoryId.Value,
            state.CategoryNames.GetValueOrDefault(model.CategoryId) ?? string.Empty, model.Name, model.Slug,
            SiteModelState.StatusText(model.Status), model.AcceptedAt, model.AcceptedBy, model.MainProductId?.Value,
            axes.Select(axis => new SiteModelAxisDto(axis.Attribute.Id.Value, axis.Attribute.Code, axis.Attribute.Name,
                SiteModelState.DisplayOf(axis.Attribute), axis.IsSaved)).ToList(),
            products, texts, Characteristics(state, modelId, axes.Select(axis => axis.Attribute.Id).ToHashSet()),
            state.ModelProblems(modelId, withProducts: false));
    }

    /// <summary>Attributes outside the axes that every on-site product of the model has with the same values.</summary>
    private static List<SiteModelCharacteristicDto> Characteristics(SiteModelState state, ProductGroupId modelId,
        IReadOnlySet<AttributeId> axes)
    {
        var products = state.OnSiteMembers(modelId).ToList();
        if (products.Count == 0)
            return [];

        return state.Values[products[0]].Select(value => value.AttributeId).Distinct()
            .Where(attributeId => !axes.Contains(attributeId) && state.Attributes.ContainsKey(attributeId))
            .Where(attributeId => products.Skip(1).All(id =>
                state.Signature(id, attributeId) == state.Signature(products[0], attributeId)))
            .Select(attributeId => state.Attributes[attributeId])
            .OrderBy(attribute => attribute.SortOrder).ThenBy(attribute => attribute.Code, StringComparer.Ordinal)
            .Select(attribute => new SiteModelCharacteristicDto(attribute.Id.Value, attribute.Name,
                state.Values[products[0]].Where(value => value.AttributeId == attribute.Id)
                    .OrderBy(value => value.Position)
                    .Select(value => value.ValueId is { } valueId
                        ? state.ValueNames.GetValueOrDefault(valueId) ?? string.Empty
                        : value.Number)
                    .Where(text => text.Length > 0)
                    .ToList()))
            .ToList();
    }

    /// <summary>
    /// On-site members only (R-18): a product that is deleted, not exported, or in a hidden category stays a row of
    /// the model but is not shown.
    /// </summary>
    private static List<ProductId> Visible(SiteModelState state, ProductGroupId modelId)
        => state.OnSiteMembers(modelId).ToList();

    private static string PhotoOf(SiteModelState state, ProductGroup model, IReadOnlyList<ProductId> members)
    {
        if (model.MainProductId is { } mainId && members.Contains(mainId) && state.Products[mainId].HasPhoto)
            return state.Products[mainId].Photo;

        return members.Select(id => state.Products[id].Photo).FirstOrDefault(photo => photo.Length > 0)
               ?? string.Empty;
    }
}
