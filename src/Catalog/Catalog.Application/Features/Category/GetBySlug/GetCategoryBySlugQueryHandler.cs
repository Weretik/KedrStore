using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.Category.GetList;
using Catalog.Application.Features.Category.GetList.DTOs;
using Catalog.Application.Features.Category.GetList.Specifications;
using Catalog.Contracts.Category;

namespace Catalog.Application.Features.Category.GetBySlug;

public sealed class GetCategoryBySlugQueryHandler(ICatalogReadRepository<ProductCategory> repository)
    : IQueryHandler<GetCategoryBySlugQuery, Result<CategoryDetailsResponse>>
{
    public async ValueTask<Result<CategoryDetailsResponse>> Handle(GetCategoryBySlugQuery query, CancellationToken cancellationToken)
    {
        var categories = await repository.ListAsync(new AllCategoriesSpec(null), cancellationToken);
        var selected = categories.FirstOrDefault(x => string.Equals(x.Slug, query.Slug, StringComparison.Ordinal));
        if (selected is null) return Result.NotFound();

        var tree = GetCategoriesQuryHandler.BuildTree(categories, query.Lang);
        var node = Find(tree, selected.Id.Value);
        if (node is null) return Result.NotFound();

        var map = categories.ToDictionary(x => x.Id.Value);
        var breadcrumbs = new List<CategoryBreadcrumbResponse>();
        var current = selected;
        var visited = new HashSet<int>();
        while (visited.Add(current.Id.Value))
        {
            var name = query.Lang == "ru" ? current.ShortNameRu : current.ShortNameUk;
            breadcrumbs.Add(new CategoryBreadcrumbResponse(current.Id.Value, current.Slug, name));
            if (current.ParentId is null || !map.TryGetValue(current.ParentId.Value.Value, out var parent)) break;
            current = parent;
        }
        breadcrumbs.Reverse();

        var response = CategoryReadMapper.ToResponse(node);
        return Result.Success(new CategoryDetailsResponse(
            response.Id, response.Slug, response.Name, response.ParentId, response.SortOrder,
            response.Level, response.Children, breadcrumbs));
    }

    private static CategoryTreeDto? Find(IEnumerable<CategoryTreeDto> nodes, int id)
    {
        foreach (var node in nodes)
        {
            if (node.Id == id) return node;
            var found = Find(node.Children, id);
            if (found is not null) return found;
        }
        return null;
    }
}
