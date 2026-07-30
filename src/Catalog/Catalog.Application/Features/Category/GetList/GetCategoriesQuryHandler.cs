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
        IReadOnlyList<CategoryTreeDto> tree = BuildTree(categories, query.Lang);
        return Result.Success(tree);
    }

    internal static IReadOnlyList<CategoryTreeDto> BuildTree(IEnumerable<ProductCategory> categories, string lang)
    {

        var items = categories.ToList()
            .Select(c => new
            {
                Id = c.Id.Value,
                c.Slug,
                c.Name,
                ParentId = c.ParentId?.Value,
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
        var byPath = items.ToDictionary(i => i.Path, StringComparer.Ordinal);
        var lookup = items.ToLookup(i => ParentKey(i.Path));

        IReadOnlyList<CategoryTreeDto> BuildBranch(string? parentKey, HashSet<string> ancestors)
        {
            return lookup[parentKey]
                .OrderBy(i => i.SortOrder)
                .ThenBy(i => i.Id)
                .Select(i => new CategoryTreeDto(
                    Id: i.Id,
                    Slug: i.Slug,
                    Name: string.Equals(lang, "ru", StringComparison.OrdinalIgnoreCase) ? i.ShortNameRu : i.ShortNameUk,
                    ParentId: i.ParentId ?? (ParentKey(i.Path) is { } parentPath && byPath.TryGetValue(parentPath, out var parent) ? parent.Id : null),
                    ShortNameUk: i.ShortNameUk,
                    ShortNameRu: i.ShortNameRu,
                    SortOrder: i.SortOrder,
                    Level: i.Level,
                    Children: ancestors.Contains(i.Path)
                        ? []
                        : BuildBranch(i.Path, new HashSet<string>(ancestors, StringComparer.Ordinal) { i.Path })
                ))
                .ToList();
        }

        return BuildBranch(null, new HashSet<string>(StringComparer.Ordinal));
    }
}
