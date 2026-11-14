using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

using Catalog.Api.Endpoints.Groups.CreateAdminProductGroup;

using Catalog.Api.Endpoints.Groups.GetAdminProductGroups;
using Catalog.Api.Endpoints.Groups.ChangeAdminProductGroupSlug;
using Catalog.Api.Endpoints.Groups.GetAdminProductGroup;
using Catalog.Api.Endpoints.Groups.SetAdminProductGroupMain;
using Catalog.Api.Endpoints.Groups.RenameAdminProductGroup;

namespace Catalog.Api.Endpoints.Groups;

/// <summary>
/// Variant groups (spec catalog/006): Manager and Admin edit them; only Admin and Seo change a group slug.
/// </summary>
public static class GroupsEndpoints
{
    public static IEndpointRouteBuilder MapGroupsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var adminProductGroups = endpoints.MapGroup("api/admin/product-groups")
            .WithTags("AdminProductGroups")
            .RequireAuthorization(PolicyNames.CanManageProducts);
        GetAdminProductGroupsEndpoint.Map(adminProductGroups);
        GetAdminProductGroupEndpoint.Map(adminProductGroups);
        CreateAdminProductGroupEndpoint.Map(adminProductGroups);
        RenameAdminProductGroupEndpoint.Map(adminProductGroups);
        SetAdminProductGroupMainEndpoint.Map(adminProductGroups);
        ChangeAdminProductGroupSlugEndpoint.Map(adminProductGroups);

        return endpoints;
    }
}
