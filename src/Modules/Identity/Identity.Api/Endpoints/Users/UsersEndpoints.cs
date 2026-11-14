using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Identity.Api.Endpoints.Users;

/// <summary>
/// Staff accounts (administrators and managers), managed by an administrator.
/// Name, email, password and avatar can be changed for managers only (403 for an administrator).
/// </summary>
public static class UsersEndpoints
{
    public static IEndpointRouteBuilder MapUsersEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var admin = endpoints.MapGroup("api/admin/users")
            .WithTags("AdminUsers")
            .RequireAuthorization(PolicyNames.CanManageUsers);
        GetStaffAccountsEndpoint.Map(admin);
        CreateStaffAccountEndpoint.Map(admin);
        SearchStaffCandidatesEndpoint.Map(admin);
        GrantStaffAccessEndpoint.Map(admin);
        ChangeStaffRoleEndpoint.Map(admin);
        SetStaffBlockedEndpoint.Map(admin);
        SetStaffPasswordEndpoint.Map(admin);
        ChangeStaffNameEndpoint.Map(admin);
        ChangeStaffEmailEndpoint.Map(admin);
        UploadStaffAvatarEndpoint.Map(admin);
        RemoveStaffAvatarEndpoint.Map(admin);

        return endpoints;
    }
}
