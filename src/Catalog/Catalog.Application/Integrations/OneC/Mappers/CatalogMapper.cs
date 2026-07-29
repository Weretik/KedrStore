using Catalog.Application.Integrations.OneC.DTOs;
using Unidecode.NET;


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
        var helper = new SlugHelper();

        if (rootCategoryId is { } configuredRootId)
        {
            var rootName = isHardwareRoot ? "Фурнітра" : "Двері";
            categoryDtos.Add(new CategoryDto(
                configuredRootId,
                rootCategoryOneCId,
                rootName,
                rootName.SlugGenerate(configuredRootId, "category", helper),
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

            var slug = name.SlugGenerate(item.CategoryId,"category", helper);
            int? mappedParentId = item.ParentId is > 0
                ? item.ParentId.Value
                : rootCategoryId;

            if (mappedParentId == id)
                mappedParentId = rootCategoryId;

            var path = BuildPath(id, new HashSet<int>());

            categoryDtos.Add(new CategoryDto(id, rootCategoryOneCId, name, slug, mappedParentId, path));
        }
        return categoryDtos;
    }
    public static IReadOnlyList<ProductRowOneCDto> MapProduct(
        IReadOnlyList<OneCProductDto> productListOneC,
        Dictionary<string, int> categoryNameDictionary,
        string rootCategoryOneCId,
        int? fallbackCategoryId = null)
    {
        var productsDtos = new List<ProductRowOneCDto>();
        var helper = new SlugHelper();

        foreach (var item in productListOneC)
        {
            var id = item.Id;
            var name = item.Name.Trim();

            var categoryId = fallbackCategoryId
                ?? GetCategoryIdForCategoryName(item.CategoryPath.Trim(), categoryNameDictionary);

            var photo = $"https://images-kedr.cdn.express/products/{id}.jpg";
            string? scheme = $"https://images-kedr.cdn.express/product-scheme/s{id}.jpg";
            int stock = 0;
            bool isSale = item.IsSale;
            bool isNew = item.IsNew;
            var qtyInPack = item.QuantityInPack;


            productsDtos.Add(new ProductRowOneCDto(
                Id: id,
                ProductTypeIdOneC: rootCategoryOneCId,
                Name: name,
                ProducSlug: name.SlugGenerate(id, "product", helper),
                CategoryId: categoryId,
                Photo: photo,
                Scheme: scheme,
                Stock: stock,
                IsSale: isSale,
                IsNew: isNew,
                ExportToSite: item.ExportToSite,
                QuantityInPack: qtyInPack)
            );
        }
        return productsDtos
            .GroupBy(product => product.Id)
            .Select(group => group.First())
            .ToList();
    }

    private static int GetCategoryIdForCategoryName(string categoryName, Dictionary<string,int> categoryNameDictionary)
        => categoryNameDictionary[categoryName];

    private static string SlugGenerate(this string s, int id, string fallbackSlugBase, SlugHelper helper)
    {
        var ascii = (s ?? string.Empty).Unidecode();

        var slugPart = helper.GenerateSlug(ascii).Trim('-');

        if (string.IsNullOrWhiteSpace(slugPart))
            slugPart = fallbackSlugBase.Trim('-');

        return $"{slugPart}-{id}";
    }



}
