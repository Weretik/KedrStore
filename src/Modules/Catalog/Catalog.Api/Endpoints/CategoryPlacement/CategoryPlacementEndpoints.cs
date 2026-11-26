using Catalog.Api.Endpoints.CategoryPlacement.AddAdminCategorySiteModels;
using Catalog.Api.Endpoints.CategoryPlacement.CreateAdminCategory;
using Catalog.Api.Endpoints.CategoryPlacement.DeleteAdminCategory;
using Catalog.Api.Endpoints.CategoryPlacement.MoveAdminCategory;
using Catalog.Api.Endpoints.CategoryPlacement.ReorderAdminCategory;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Api.Endpoints.CategoryPlacement;

/// <summary>Categories made and placed by hand (spec catalog/012), in the admin categories group.</summary>
internal static class CategoryPlacementEndpoints
{
    public static void Map(RouteGroupBuilder adminCategories)
    {
        CreateAdminCategoryEndpoint.Map(adminCategories);
        MoveAdminCategoryEndpoint.Map(adminCategories);
        ReorderAdminCategoryEndpoint.Map(adminCategories);
        DeleteAdminCategoryEndpoint.Map(adminCategories);
        AddAdminCategorySiteModelsEndpoint.Map(adminCategories);
    }
}
