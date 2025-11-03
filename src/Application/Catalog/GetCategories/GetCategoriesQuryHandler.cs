using Application.Catalog.ImportCatalogFromXml;
using Application.Catalog.Shared;
using Domain.Catalog.Entities;

namespace Application.Catalog.GetCategories;

public class GetCategoriesQuryHandler(ICatalogReadRepository<ProductCategory> categoryRepository)
    : IQueryHandler<GetCategoriesQuery, Result<IReadOnlyList<CategoryTreeDto>>>
{
    public async ValueTask<Result<IReadOnlyList<CategoryTreeDto>>> Handle(GetCategoriesQuery query, CancellationToken cancellationToken)
    {
        var categories = await categoryRepository.ListAsync(new AllCategoriesSpec(), cancellationToken);
        if (categories.Count == 0) return Result.NotFound();

        IReadOnlyList<CategoryTreeDto> tree = BuildTree(categories);
        return Result.Success(tree);
    }

    private static IReadOnlyList<CategoryTreeDto> BuildTree(IEnumerable<ProductCategory> categories)
    {
        var lookup = categories.ToLookup(
            category =>
                category.TryGetParentPath(out var parentPath)
                ? parentPath.ToString()
                : null
        );

        IReadOnlyList<CategoryTreeDto> BuildBranch(string? parentKey)
        {
            return lookup[parentKey]
                .Select(category => new CategoryTreeDto(
                    Id: category.Id.Value,
                    Name: category.Name,
                    Children: BuildBranch(category.Path.ToString())
                ))
                .ToList();
        }

        return BuildBranch(null);
    }
}
