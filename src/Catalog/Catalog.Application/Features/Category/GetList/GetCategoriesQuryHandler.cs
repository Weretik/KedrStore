using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Features.Category.GetList.DTOs;
using Catalog.Application.Features.Category.GetList.Specifications;
using Catalog.Domain.Entities;

namespace Catalog.Application.Features.Category.GetList;

public class GetCategoriesQuryHandler(ICatalogReadRepository<ProductCategory> categoryRepository)
    : IQueryHandler<GetCategoriesQuery, Result<IReadOnlyList<CategoryTreeDto>>>
{
    public async ValueTask<Result<IReadOnlyList<CategoryTreeDto>>> Handle(GetCategoriesQuery query, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);

        var categories = await categoryRepository.ListAsync(new AllCategoriesSpec(query.Filter.ProductTypeId), cancellationToken);
        if (categories.Count == 0) return Result.NotFound();

        IReadOnlyList<CategoryTreeDto> tree = BuildTree(categories);
        return Result.Success(tree);
    }

    private static IReadOnlyList<CategoryTreeDto> BuildTree(IEnumerable<ProductCategory> categories)
    {

        var items = categories.ToList()
            .Select(c => new
            {
                Id = c.Id.Value,
                c.Name,
                c.ShortNameUk,
                c.ShortNameRu,
                c.SortOrder,
                c.Level,
                Path = c.Path.Value
            })
            .ToList();

        static string? ParentKey(string path)
        {
            var i = path.LastIndexOf('.');
            return i >= 0 ? path[..i] : null;
        }
        var lookup = items.ToLookup(i => ParentKey(i.Path));

        IReadOnlyList<CategoryTreeDto> BuildBranch(string? parentKey)
        {
            return lookup[parentKey]
                .OrderBy(i => i.SortOrder)
                .ThenBy(i => i.Id)
                .Select(i => new CategoryTreeDto(
                    Id: i.Id,
                    Name: i.Name,
                    ShortNameUk: i.ShortNameUk,
                    ShortNameRu: i.ShortNameRu,
                    SortOrder: i.SortOrder,
                    Level: i.Level,
                    Children: BuildBranch(i.Path)
                ))
                .ToList();
        }

        return BuildBranch(null);
    }
}
