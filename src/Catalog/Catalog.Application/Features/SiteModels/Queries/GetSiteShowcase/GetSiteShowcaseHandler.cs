using Catalog.Application.Features.SiteModels.Shared;
using Catalog.Contracts.SiteModels;
using Catalog.Domain.Groups;
using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;

namespace Catalog.Application.Features.SiteModels.Queries.GetSiteShowcase;

/// <summary>
/// «Вітрина сайту» (spec catalog/007 M-13, R-19): one row per model with on-site products, and one row per on-site
/// product in no model; each row carries its on-site products with the confirmed variant values and problems (R-08).
/// Rows with the hard error come first, then rows with any problem, then by name.
/// </summary>
public sealed class GetSiteShowcaseHandler(SiteModelStateReader reader)
    : IQueryHandler<GetSiteShowcaseQuery, Result<SiteShowcaseListDto>>
{
    public async ValueTask<Result<SiteShowcaseListDto>> Handle(GetSiteShowcaseQuery query,
        CancellationToken cancellationToken)
    {
        var request = query.Request;
        var state = await reader.ReadAsync(cancellationToken);
        var search = request.Search?.Trim();
        var variantAttributes = state.Attributes.Values.Where(attribute => attribute.IsVariantAxis)
            .OrderBy(attribute => attribute.SortOrder).ThenBy(attribute => attribute.Code, StringComparer.Ordinal)
            .ToList();

        var modelRows = state.Models.Values
            .Select(model => (Model: (ProductGroup?)model, CategoryId: model.CategoryId, Name: model.Name,
                Products: state.OnSiteMembers(model.Id).ToList()));
        var loneRows = state.Products.Values
            .Where(product => product.IsOnSite && !state.ModelOfProduct.ContainsKey(product.Id))
            .Select(product => (Model: (ProductGroup?)null, product.CategoryId, product.Name,
                Products: new List<ProductId> { product.Id }));

        var rows = modelRows.Concat(loneRows)
            .Where(row => row.Products.Count > 0)
            .Where(row => request.Status is null || StatusOf(row.Model) == request.Status)
            .Where(row => state.InCategory(row.CategoryId, request.CategoryId))
            .Where(row => string.IsNullOrEmpty(search) || Matches(state, row.Name, row.Products, search))
            .Select(row => ToRow(state, variantAttributes, row.Model, row.CategoryId, row.Name, row.Products))
            .Where(row => SiteModelProblems.Passes(row.Problems, request.Problem))
            .OrderByDescending(row => row.Problems.Contains(SiteModelProblems.NoModel))
            .ThenByDescending(row => row.Problems.Count > 0)
            .ThenBy(row => row.Name, StringComparer.CurrentCulture)
            .ThenBy(row => row.Key, StringComparer.Ordinal)
            .ToList();

        var page = rows.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList();
        return Result.Success(new SiteShowcaseListDto(page, rows.Count, request.Page, request.PageSize));
    }

    private static string StatusOf(ProductGroup? model)
        => model is null ? GetSiteShowcaseQuery.NoModelStatus : SiteModelState.StatusText(model.Status);

    private static bool Matches(SiteModelState state, string name, IReadOnlyList<ProductId> products, string search)
        => name.Contains(search, StringComparison.CurrentCultureIgnoreCase) ||
           products.Any(id => id.Value.ToString() == search ||
                              state.Products[id].Name.Contains(search, StringComparison.CurrentCultureIgnoreCase));

    private static SiteShowcaseRowDto ToRow(SiteModelState state, IReadOnlyList<SiteModelAttributeFacts> variantAttributes,
        ProductGroup? model, ProductCategoryId categoryId, string name, IReadOnlyList<ProductId> productIds)
    {
        var modelDto = model is null
            ? null
            : new SiteShowcaseModelDto(model.Id.Value, model.Name, SiteModelState.StatusText(model.Status));
        var products = productIds
            .OrderByDescending(id => model?.MainProductId == id)
            .ThenBy(id => state.Products[id].Name, StringComparer.CurrentCulture)
            .Select(id => ToItem(state, variantAttributes, id, modelDto))
            .ToList();
        var problems = model is null
            ? state.ProductProblems(productIds[0])
            : state.ModelProblems(model.Id);
        var photo = products.Select(product => product.Photo).FirstOrDefault(value => value.Length > 0) ?? string.Empty;

        return new SiteShowcaseRowDto(model is null ? $"p{productIds[0].Value}" : $"m{model.Id.Value}", modelDto,
            name, photo, categoryId.Value, state.CategoryNames.GetValueOrDefault(categoryId) ?? string.Empty,
            products.Select(product => product.RetailPrice).Where(price => price is not null).Min(),
            products.Any(product => product.InStock), products, problems);
    }

    private static SiteShowcaseItemDto ToItem(SiteModelState state, IReadOnlyList<SiteModelAttributeFacts> variantAttributes,
        ProductId id, SiteShowcaseModelDto? model)
    {
        var product = state.Products[id];
        var values = variantAttributes.SelectMany(attribute => state.Values[id]
                .Where(value => value.AttributeId == attribute.Id)
                .OrderBy(value => value.Position)
                .Select(value => new SiteShowcaseValueDto(attribute.Id.Value, attribute.Name,
                    value.ValueId is { } valueId ? state.ValueNames.GetValueOrDefault(valueId) ?? "" : value.Number)))
            .ToList();
        return new SiteShowcaseItemDto(product.Id.Value, product.Name, product.Slug, product.Photo,
            product.CategoryId.Value, state.CategoryNames.GetValueOrDefault(product.CategoryId) ?? string.Empty,
            product.RetailPrice, product.InStock, model, values, state.ProductProblems(id));
    }
}
