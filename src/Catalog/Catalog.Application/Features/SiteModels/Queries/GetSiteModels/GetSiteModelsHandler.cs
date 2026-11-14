using Catalog.Application.Features.SiteModels.Shared;
using Catalog.Contracts.SiteModels;
using Catalog.Domain.Attributes;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.PriceTypes;
using Catalog.Domain.Products;
using Catalog.Domain.Products.Media;
using Catalog.Domain.Slugs;

namespace Catalog.Application.Features.SiteModels.Queries.GetSiteModels;

/// <summary>
/// The «Моделі» list: models with on-site products (R-08, R-18), problems first, then by name. The category filter takes
/// the category with its subcategories. The search matches the model name, an on-site product name, or its 1C code.
/// </summary>
public sealed class GetSiteModelsHandler(SiteModelStateReader reader)
    : IQueryHandler<GetSiteModelsQuery, Result<SiteModelListDto>>
{
    public async ValueTask<Result<SiteModelListDto>> Handle(GetSiteModelsQuery query,
        CancellationToken cancellationToken)
    {
        var request = query.Request;
        var state = await reader.ReadAsync(cancellationToken);
        var search = request.Search?.Trim();

        var rows = state.Models.Values
            .Where(model => state.InCategory(model.CategoryId, request.CategoryId))
            .Where(model => request.Status is null || SiteModelState.StatusText(model.Status) == request.Status)
            .Where(model => state.OnSiteMembers(model.Id).Any())
            .Where(model => string.IsNullOrEmpty(search) || Matches(state, model.Id, model.Name, search))
            .Select(model => SiteModelMapper.ToListItem(state, model.Id))
            .Where(item => SiteModelProblems.Passes(item.Problems, request.Problem))
            .OrderByDescending(item => item.Problems.Count > 0)
            .ThenBy(item => item.Name, StringComparer.CurrentCulture)
            .ThenBy(item => item.Id)
            .ToList();

        var page = rows.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList();
        return Result.Success(new SiteModelListDto(page, rows.Count, request.Page, request.PageSize));
    }

    private static bool Matches(SiteModelState state, ProductGroupId modelId, string name, string search)
        => name.Contains(search, StringComparison.CurrentCultureIgnoreCase) ||
           state.OnSiteMembers(modelId).Any(id =>
               id.Value.ToString() == search ||
               state.Products[id].Name.Contains(search, StringComparison.CurrentCultureIgnoreCase));
}
