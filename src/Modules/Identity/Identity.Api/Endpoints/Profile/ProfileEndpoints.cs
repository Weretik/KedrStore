using Identity.Contracts.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace Identity.Api.Endpoints.Profile;

/// <summary>
/// The signed-in staff account's own profile. Managers change name and avatar;
/// administrators also change email and password.
/// </summary>
public static class ProfileEndpoints
{
    public static IEndpointRouteBuilder MapProfileEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var profile = endpoints.MapGroup("api/profile")
            .WithTags("Profile")
            .RequireAuthorization(PolicyNames.RequireManagerRole);
        GetMyProfileEndpoint.Map(profile);
        ChangeMyNameEndpoint.Map(profile);
        UploadMyAvatarEndpoint.Map(profile);
        RemoveMyAvatarEndpoint.Map(profile);
        ChangeMyEmailEndpoint.Map(profile);
        ChangeMyPasswordEndpoint.Map(profile);

        return endpoints;
    }
}
