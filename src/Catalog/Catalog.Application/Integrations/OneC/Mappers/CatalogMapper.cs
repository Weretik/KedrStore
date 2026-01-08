using BuildingBlocks.Application.Integrations.OneC.DTOs;
using BuildingBlocks.Application.Integrations.OneC.RootCategoryId;
using Catalog.Application.Features.Import.Queries.ImportCatalogFromXml;
using Catalog.Application.Features.Shared;
using Catalog.Application.Shared;
using Catalog.Domain.ValueObjects;
using Microsoft.Extensions.Options;
using Slugify;

namespace Catalog.Application.Integrations.OneC.Mappers;

public static class CatalogMapper
{
    public static IReadOnlyList<CategoryDto> MapCategory(
        IReadOnlyList<OneCCategoryDto> categoryListOneC,
        int rootCategoryId,
        int furnitureId)
    {
        var categoryDtos = new List<CategoryDto>();
        var helper = new SlugHelper();

        string nameRootCategory = rootCategoryId == furnitureId ? "Фурнітра" : "Двері";

        categoryDtos.Add(new CategoryDto(
            rootCategoryId,
            nameRootCategory ,
            helper.GenerateSlug(nameRootCategory),
            null,
            $"n{rootCategoryId}")
        );

        foreach (var item in categoryListOneC)
        {
            var id = int.Parse(item.CategoryId.TrimStart('0'));
            var name = item.CategoryName.Trim();
            var slug = helper.GenerateSlug(name);
            var parentId = rootCategoryId;
            var path = $"n{rootCategoryId}.n{id}";

            categoryDtos.Add(new CategoryDto(id, name, slug, parentId, path));
        }
        return categoryDtos;
    }
    public static IReadOnlyList<ProductDto> MapProduct(IReadOnlyList<OneCProductDto> productListOneC)
    {
        var productsDtos = new List<ProductDto>();
        var helper = new SlugHelper();

        foreach (var item in productListOneC)
        {
            //if (!item.ExportToSite) continue;

            var id = int.Parse(item.Id.TrimStart('0'));
            var name = item.Name.Trim();
            var categorySlug= helper.GenerateSlug(item.CategoryPath.Trim());
            var photo = $"https://images-kedr.cdn.express/products/{id}.jpg";
            string? scheme = $"https://images-kedr.cdn.express/product-scheme/s{id}.jpg";
            int stock = 0;
            bool isSale = item.IsSale;
            bool isNew = item.IsNew;
            var qtyInPack = item.QuantityInPack;
            List<ProductPriceDto> prices = [];


            productsDtos.Add(new ProductDto(
                Id: id,
                Name: name,
                CategorySlug: categorySlug,
                Photo: photo,
                Scheme: scheme,
                Stock: stock,
                IsSale: isSale,
                IsNew: isNew,
                QuantityInPack: qtyInPack,
                Prices: prices)
            );
        }
        return productsDtos;
    }
}
