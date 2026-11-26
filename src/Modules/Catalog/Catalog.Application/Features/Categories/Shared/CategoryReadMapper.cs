using Catalog.Application.Features.Categories.Queries.GetCategories;
using Catalog.Contracts.Category;
using Catalog.Application.Features.Categories.Shared.Tree;

namespace Catalog.Application.Features.Categories.Shared;

public static class CategoryReadMapper
{
    public static CategoryResponse ToResponse(CategoryTreeDto dto) => new(
        dto.Id, dto.Slug, dto.Name, dto.ParentId, dto.SortOrder, dto.Level,
        dto.Children.Select(ToResponse).ToList(), dto.ProductCount)
    {
        IsClosedFromSearch = dto.IsClosedFromSearch,
        UpdatedAt = dto.UpdatedAt
    };

    public static AdminCategoryResponse ToAdminResponse(ProductCategory category, string? shareImageUrl = null) => new(
        category.Id.Value, category.Name, category.ShortNameUk, category.ShortNameRu, category.Slug,
        category.ProductTypeIdOneC, category.ParentId?.Value, category.SortOrder, category.Level,
        category.IsDeleted, category.DeletedAt, category.IsShownOnSite)
    {
        SiteNameUk = category.SiteNameUk,
        SiteNameRu = category.SiteNameRu,
        IsClosedFromSearch = category.IsClosedFromSearch,
        ShareImageMediaFileId = category.ShareImageMediaFileId?.Value,
        ShareImageUrl = shareImageUrl,
        IsManual = category.IsManual
    };
}
