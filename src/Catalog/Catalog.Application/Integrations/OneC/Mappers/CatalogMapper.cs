using Catalog.Application.Integrations.OneC.DTOs;
using Catalog.Application.Integrations.OneC.Sync;

namespace Catalog.Application.Integrations.OneC.Mappers;

public static class CatalogMapper
{
    public static IReadOnlyList<CategoryDto> MapCategory(
        IReadOnlyList<OneCCategoryDto> categoryListOneC,
        int? rootCategoryId,
        bool isHardwareRoot,
        string rootCategoryOneCId)
    {
        var categoryDtos = new List<CategoryDto>();

        if (rootCategoryId is { } configuredRootId)
        {
            var rootName = isHardwareRoot ? "Фурнітра" : "Двері";
            categoryDtos.Add(new CategoryDto(
                configuredRootId,
                rootCategoryOneCId,
                rootName,
                null,
                $"n{configuredRootId}"));
        }

        var categoriesById = categoryListOneC
            .Where(category => category.CategoryId > 0)
            .GroupBy(category => category.CategoryId)
            .ToDictionary(group => group.Key, group => group.First());

        var pathCache = new Dictionary<int, string>();
        if (rootCategoryId is { } rootId)
            pathCache[rootId] = $"n{rootId}";

        string BuildPath(int categoryId, HashSet<int> visiting)
        {
            if (pathCache.TryGetValue(categoryId, out var cachedPath))
                return cachedPath;

            if (!categoriesById.TryGetValue(categoryId, out var category))
                return RootPath(categoryId);

            var parentId = category.ParentId;
            string path;

            if (parentId is null || parentId <= 0 || parentId == categoryId)
            {
                path = RootPath(categoryId);
            }
            else if (parentId == rootCategoryId)
            {
                path = $"n{rootCategoryId}.n{categoryId}";
            }
            else if (!visiting.Add(categoryId))
            {
                path = RootPath(categoryId);
            }
            else
            {
                var parentPath = BuildPath(parentId.Value, visiting);
                visiting.Remove(categoryId);
                path = $"{parentPath}.n{categoryId}";
            }

            pathCache[categoryId] = path;
            return path;
        }

        string RootPath(int categoryId)
            => rootCategoryId is { } rootId
                ? $"n{rootId}.n{categoryId}"
                : $"n{categoryId}";

        foreach (var item in categoryListOneC)
        {
            var id = item.CategoryId;
            if (id <= 0 || id == rootCategoryId)
                continue;

            var name = (item.CategoryName ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(name))
                continue;

            int? mappedParentId = item.ParentId is > 0
                ? item.ParentId.Value
                : rootCategoryId;

            if (mappedParentId == id)
                mappedParentId = rootCategoryId;

            var path = BuildPath(id, new HashSet<int>());

            categoryDtos.Add(new CategoryDto(id, rootCategoryOneCId, name, mappedParentId, path));
        }
        return categoryDtos;
    }
    /// <summary>
    /// Maps 1C products. With <paramref name="fallbackCategoryId"/> every product goes to that category;
    /// otherwise the category is resolved by name and stays <c>null</c> when the name does not resolve.
    /// </summary>
    public static IReadOnlyList<ProductRowOneCDto> MapProduct(
        IReadOnlyList<OneCProductDto> productListOneC,
        CategoryNameMap? categoryNames,
        string rootCategoryOneCId,
        int? fallbackCategoryId = null)
    {
        var productsDtos = new List<ProductRowOneCDto>(productListOneC.Count);

        foreach (var item in productListOneC)
        {
            var id = item.Id;
            var categoryId = fallbackCategoryId ?? categoryNames?.Resolve(item.CategoryPath);

            productsDtos.Add(new ProductRowOneCDto(
                Id: id,
                ProductTypeIdOneC: rootCategoryOneCId,
                Name: item.Name.Trim(),
                CategoryId: categoryId,
                Photo: $"https://images-kedr.cdn.express/products/{id}.jpg",
                Scheme: $"https://images-kedr.cdn.express/product-scheme/s{id}.jpg",
                Stock: 0,
                IsSale: item.IsSale,
                IsNew: item.IsNew,
                ExportToSite: item.ExportToSite,
                QuantityInPack: item.QuantityInPack));
        }

        return productsDtos
            .GroupBy(product => product.Id)
            .Select(group => group.First())
            .ToList();
    }
}
