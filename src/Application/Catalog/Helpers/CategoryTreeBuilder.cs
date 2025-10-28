namespace Application.Catalog.Helpers;

public static class ProductCategoryTreeBuilder
{
    public static IReadOnlyList<CategoryTreeDto> BuildTree(IEnumerable<ProductCategory> categories)
    {
        var lookup = categories.ToLookup(
            productCategory => productCategory.TryGetParentPath(out var parent)
                ? parent.ToString()
                : string.Empty);

        List<CategoryTreeDto> BuildBranch(string parentKey)
        {
            return lookup[parentKey]
                .OrderBy(productCategory => productCategory.Path.ToString())
                .Select(productCategory =>
                {
                    var dto  = productCategory.Adapt<CategoryTreeDto>() with
                    {
                        Children = BuildBranch(productCategory.Path.ToString())
                    };
                    return dto;
                })
                .ToList();
        }

        return BuildBranch(string.Empty);
    }
}
