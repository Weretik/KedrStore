using Catalog.Application.Features.Category.GetList.DTOs;
using Catalog.Contracts.Category;

namespace Catalog.Application.Features.Category;

public static class CategoryReadMapper
{
    public static CategoryResponse ToResponse(CategoryTreeDto dto) => new(
        dto.Id, dto.Slug, dto.Name, dto.ParentId, dto.SortOrder, dto.Level,
        dto.Children.Select(ToResponse).ToList());

    public static AdminCategoryResponse ToAdminResponse(ProductCategory category) => new(
        category.Id.Value, category.Name, category.ShortNameUk, category.ShortNameRu, category.Slug,
        category.ProductTypeIdOneC, category.ParentId?.Value, category.SortOrder, category.Level);
}
