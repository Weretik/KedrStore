using Catalog.Application.Features.SiteModels.Common;
using Catalog.Contracts.SiteModels;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Features.SiteModels.GetList;

/// <summary>
/// The «Моделі» list: models with products that are not deleted, problems first, then by name. The search matches the
/// model name, a product name, or a product's 1C code.
/// </summary>
public sealed class GetSiteModelsQueryHandler(SiteModelStateReader reader)
    : IQueryHandler<GetSiteModelsQuery, Result<SiteModelListDto>>
{
    public async ValueTask<Result<SiteModelListDto>> Handle(GetSiteModelsQuery query,
        CancellationToken cancellationToken)
    {
        var request = query.Request;
        var state = await reader.ReadAsync(cancellationToken);
        var search = request.Search?.Trim();

        var rows = state.Models.Values
            .Where(model => request.CategoryId is not { } categoryId || model.CategoryId.Value == categoryId)
            .Where(model => request.Status is null || SiteModelState.StatusText(model.Status) == request.Status)
            .Where(model => state.MembersByModel[model.Id].Any(id => !state.Products[id].IsDeleted))
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
           state.MembersByModel[modelId].Any(id =>
               id.Value.ToString() == search ||
               state.Products[id].Name.Contains(search, StringComparison.CurrentCultureIgnoreCase));
}
