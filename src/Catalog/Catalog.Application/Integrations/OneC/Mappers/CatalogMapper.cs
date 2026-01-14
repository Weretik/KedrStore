using Catalog.Application.Integrations.OneC.DTOs;

namespace Catalog.Application.Integrations.OneC.Mappers;

public static class CatalogMapper
{
    public static IReadOnlyList<CategoryDto> MapCategory(
        IReadOnlyList<OneCCategoryDto> categoryListOneC,
        int rootCategoryId,
        int furnitureId,
        string rootCategoryOneCId)
    {
        var categoryDtos = new List<CategoryDto>();
        var helper = new SlugHelper();

        string nameRootCategory = rootCategoryId == furnitureId ? "Фурнітра" : "Двері";

        categoryDtos.Add(new CategoryDto(
            rootCategoryId,
            rootCategoryOneCId,
            nameRootCategory ,
            helper.GenerateSlug(nameRootCategory),
            null,
            $"n{rootCategoryId}")
        );

        foreach (var item in categoryListOneC)
        {
            var id = item.CategoryId;
            var name = item.CategoryName.Trim();
            var slug = helper.GenerateSlug(name);
            var parentId = rootCategoryId;
            var path = $"n{rootCategoryId}.n{id}";

            categoryDtos.Add(new CategoryDto(id, rootCategoryOneCId, name, slug, parentId, path));
        }
        return categoryDtos;
    }
    public static IReadOnlyList<ProductDto> MapProduct(
        IReadOnlyList<OneCProductDto> productListOneC,
        Dictionary<string, int> slugDictionary,
        string rootCategoryOneCId)
    {
        var productsDtos = new List<ProductDto>();
        var helper = new SlugHelper();

        foreach (var item in productListOneC)
        {
            //if (!item.ExportToSite) continue;

            var id = item.Id;
            var name = item.Name.Trim();

            var categoryId = GetCategoryIdForSlug(
                    helper.GenerateSlug(item.CategoryPath.Trim()), slugDictionary);

            var photo = $"https://images-kedr.cdn.express/products/{id}.jpg";
            string? scheme = $"https://images-kedr.cdn.express/product-scheme/s{id}.jpg";
            int stock = 0;
            bool isSale = item.IsSale;
            bool isNew = item.IsNew;
            var qtyInPack = item.QuantityInPack;


            productsDtos.Add(new ProductDto(
                Id: id,
                ProductTypeIdOneC: rootCategoryOneCId,
                Name: name,
                ProducSlug: helper.GenerateSlug(name),
                CategoryId: categoryId,
                Photo: photo,
                Scheme: scheme,
                Stock: stock,
                IsSale: isSale,
                IsNew: isNew,
                QuantityInPack: qtyInPack)
            );
        }
        return productsDtos;
    }

    private static int GetCategoryIdForSlug(string slug, Dictionary<string,int> slugDictionary)
        => slugDictionary[slug];

}
