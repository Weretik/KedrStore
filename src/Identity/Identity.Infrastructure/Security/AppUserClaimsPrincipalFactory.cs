using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Identity.Infrastructure.Entities;

namespace Identity.Infrastructure.Security;

public sealed class AppUserClaimsPrincipalFactory(
    UserManager<AppUser> userManager,
    RoleManager<AppRole> roleManager,
    IOptions<IdentityOptions> optionsAccessor)
    : UserClaimsPrincipalFactory<AppUser, AppRole>(userManager, roleManager, optionsAccessor)
{
    private readonly IdentityOptions _identityOptions = optionsAccessor.Value;

    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(AppUser user)
    {
        var identity = new ClaimsIdentity(
            IdentityConstants.ApplicationScheme,
            _identityOptions.ClaimsIdentity.UserNameClaimType,
            _identityOptions.ClaimsIdentity.RoleClaimType);

        identity.AddClaim(new Claim(_identityOptions.ClaimsIdentity.UserIdClaimType, user.Id.ToString()));

        if (!string.IsNullOrWhiteSpace(user.UserName))
        {
            identity.AddClaim(new Claim(_identityOptions.ClaimsIdentity.UserNameClaimType, user.UserName));
        }

        if (UserManager.SupportsUserEmail && !string.IsNullOrWhiteSpace(user.Email))
        {
            identity.AddClaim(new Claim(_identityOptions.ClaimsIdentity.EmailClaimType, user.Email));
        }

        if (UserManager.SupportsUserSecurityStamp)
        {
            var securityStamp = await UserManager.GetSecurityStampAsync(user);
            if (!string.IsNullOrWhiteSpace(securityStamp))
            {
                identity.AddClaim(new Claim(_identityOptions.ClaimsIdentity.SecurityStampClaimType, securityStamp));
            }
        }

        if (UserManager.SupportsUserClaim)
        {
            identity.AddClaims(await UserManager.GetClaimsAsync(user));
        }

        if (UserManager.SupportsUserRole)
        {
            var roles = await UserManager.GetRolesAsync(user);
            foreach (var roleName in roles.Where(roleName => !string.IsNullOrWhiteSpace(roleName)))
            {
                identity.AddClaim(new Claim(_identityOptions.ClaimsIdentity.RoleClaimType, roleName));
            }
        }

        return identity;
    }
}
