using Microsoft.AspNetCore.RateLimiting;
using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Catalog.Api.Endpoints.Categories.GetAdminCategoryContent;

using Catalog.Api.Endpoints.Categories.GetCategoryBySlug;
using Catalog.Api.Endpoints.Categories.SetAdminCategorySiteVisibility;
using Catalog.Api.Endpoints.Categories.GetAdminCategory;
using Catalog.Api.Endpoints.Categories.ChangeAdminCategorySlug;
using Catalog.Api.Endpoints.Categories.GetCategories;
using Catalog.Api.Endpoints.Categories.GetAdminCategories;
using Catalog.Api.Endpoints.Categories.SaveAdminCategoryContent;
using Catalog.Api.Endpoints.Categories.SetAdminCategorySearchSettings;
using Catalog.Api.Endpoints.Categories.SetAdminCategorySiteNames;
using Catalog.Api.Endpoints.CategoryPlacement;

namespace Catalog.Api.Endpoints.Categories;

/// <summary>Categories: the admin tree, slug, site visibility, site names, search settings and content, and the public tree and category by slug.</summary>
public static class CategoriesEndpoints
{
    public static IEndpointRouteBuilder MapCategoriesEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var adminCategories = endpoints.MapGroup("api/categories")
            .WithTags("AdminCategories")
            .RequireAuthorization(PolicyNames.CanManageProducts);
        GetAdminCategoriesEndpoint.Map(adminCategories);
        GetAdminCategoryEndpoint.Map(adminCategories);
        ChangeAdminCategorySlugEndpoint.Map(adminCategories);
        SetAdminCategorySiteVisibilityEndpoint.Map(adminCategories);
        SetAdminCategorySiteNamesEndpoint.Map(adminCategories);
        SetAdminCategorySearchSettingsEndpoint.Map(adminCategories);
        GetAdminCategoryContentEndpoint.Map(adminCategories);
        SaveAdminCategoryContentEndpoint.Map(adminCategories);
        CategoryPlacementEndpoints.Map(adminCategories);

        var categories = endpoints.MapGroup("api/catalog/{lang}")
            .WithTags("Categories")
            .AllowAnonymous().RequireRateLimiting("PublicRead");
        GetCategoriesEndpoint.Map(categories);
        GetCategoryBySlugEndpoint.Map(categories);

        return endpoints;
    }
}
