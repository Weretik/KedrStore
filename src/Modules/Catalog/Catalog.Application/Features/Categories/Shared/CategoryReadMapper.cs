using Catalog.Application.Features.Categories.Queries.GetCategories;
using Catalog.Contracts.Category;
using Catalog.Application.Features.Categories.Shared.Tree;

namespace Catalog.Application.Features.Categories.Shared;

public static class CategoryReadMapper
{
    public static CategoryResponse ToResponse(CategoryTreeDto dto) => new(
        dto.Id, dto.Slug, dto.Name, dto.ParentId, dto.SortOrder, dto.Level,
        dto.Children.Select(ToResponse).ToList(), dto.ProductCount);

    public static AdminCategoryResponse ToAdminResponse(ProductCategory category) => new(
        category.Id.Value, category.Name, category.ShortNameUk, category.ShortNameRu, category.Slug,
        category.ProductTypeIdOneC, category.ParentId?.Value, category.SortOrder, category.Level,
        category.IsDeleted, category.DeletedAt, category.IsShownOnSite);
}
