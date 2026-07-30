using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.Category.GetList.Specifications;
using Catalog.Contracts.Category;

namespace Catalog.Application.Features.Category.AdminGetList;

public sealed class GetAdminCategoriesQueryHandler(ICatalogReadRepository<ProductCategory> repository)
    : IQueryHandler<GetAdminCategoriesQuery, Result<IReadOnlyList<AdminCategoryResponse>>>
{
    public async ValueTask<Result<IReadOnlyList<AdminCategoryResponse>>> Handle(GetAdminCategoriesQuery query, CancellationToken cancellationToken)
    {
        var categories = await repository.ListAsync(new AllCategoriesSpec(null), cancellationToken);
        var byId = categories.ToDictionary(x => x.Id.Value);
        var children = categories.ToLookup(x => x.ParentId?.Value);
        var result = new List<AdminCategoryResponse>();
        var visited = new HashSet<int>();

        void Visit(ProductCategory item)
        {
            if (!visited.Add(item.Id.Value)) return;
            result.Add(CategoryReadMapper.ToAdminResponse(item));
            foreach (var child in children[item.Id.Value].OrderBy(x => x.SortOrder).ThenBy(x => x.Id.Value)) Visit(child);
        }

        foreach (var root in categories.Where(x => x.ParentId is null || !byId.ContainsKey(x.ParentId.Value.Value))
                     .OrderBy(x => x.SortOrder).ThenBy(x => x.Id.Value)) Visit(root);
        foreach (var orphan in categories.OrderBy(x => x.SortOrder).ThenBy(x => x.Id.Value)) Visit(orphan);
        return Result.Success<IReadOnlyList<AdminCategoryResponse>>(result);
    }
}
