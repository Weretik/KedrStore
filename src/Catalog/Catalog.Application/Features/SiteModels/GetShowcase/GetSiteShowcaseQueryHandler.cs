using Catalog.Application.Features.SiteModels.Common;
using Catalog.Contracts.SiteModels;

namespace Catalog.Application.Features.SiteModels.GetShowcase;

/// <summary>
/// «Вітрина сайту» (spec catalog/007 M-13): on-site, not deleted products with their model, the confirmed values of the
/// variant attributes, and their problems (R-08). Products with problems come first, the hard error first of all.
/// </summary>
public sealed class GetSiteShowcaseQueryHandler(SiteModelStateReader reader)
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

        var rows = state.Products.Values
            .Where(product => product.IsOnSite)
            .Where(product => request.CategoryId is not { } categoryId || product.CategoryId.Value == categoryId)
            .Where(product => string.IsNullOrEmpty(search) || product.Id.Value.ToString() == search ||
                              product.Name.Contains(search, StringComparison.CurrentCultureIgnoreCase))
            .Select(product =>
            {
                var model = state.ModelOfProduct.TryGetValue(product.Id, out var modelId) ? state.Models[modelId] : null;
                var values = variantAttributes.SelectMany(attribute => state.Values[product.Id]
                        .Where(value => value.AttributeId == attribute.Id)
                        .OrderBy(value => value.Position)
                        .Select(value => new SiteShowcaseValueDto(attribute.Id.Value, attribute.Name,
                            value.ValueId is { } valueId ? state.ValueNames.GetValueOrDefault(valueId) ?? "" : value.Number)))
                    .ToList();
                return new SiteShowcaseItemDto(product.Id.Value, product.Name, product.Slug, product.Photo,
                    product.CategoryId.Value, state.CategoryNames.GetValueOrDefault(product.CategoryId) ?? string.Empty,
                    product.RetailPrice, product.InStock,
                    model is null ? null : new SiteShowcaseModelDto(model.Id.Value, model.Name, SiteModelState.StatusText(model.Status)),
                    values, state.ProductProblems(product.Id));
            })
            .Where(item => SiteModelProblems.Passes(item.Problems, request.Problem))
            .OrderByDescending(item => item.Problems.Contains(SiteModelProblems.NoModel))
            .ThenByDescending(item => item.Problems.Count > 0)
            .ThenBy(item => item.Name, StringComparer.CurrentCulture)
            .ThenBy(item => item.ProductId)
            .ToList();

        var page = rows.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList();
        return Result.Success(new SiteShowcaseListDto(page, rows.Count, request.Page, request.PageSize));
    }
}
