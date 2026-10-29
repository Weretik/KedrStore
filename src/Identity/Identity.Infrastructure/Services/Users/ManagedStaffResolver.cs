using Ardalis.Result;
using Identity.Application.Features.Users.Shared;
using Identity.Domain.Authorization;
using Identity.Infrastructure.Entities;

namespace Identity.Infrastructure.Services.Users;

/// <summary>
/// Finds the account an administrator edits on the Users page. Only managers can be edited;
/// an administrator account (including the acting one) is forbidden.
/// </summary>
public sealed class ManagedStaffResolver(UserManager<AppUser> userManager)
{
    public async Task<ManagedStaff> ResolveAsync(Guid userId)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return new ManagedStaff(null, Result.NotFound());
        }

        var roles = await userManager.GetRolesAsync(user);
        if (!roles.Any(StaffRoles.IsStaffRole))
        {
            return new ManagedStaff(null, Result.NotFound());
        }

        return roles.Contains(RoleNames.Admin, StringComparer.Ordinal)
            ? new ManagedStaff(null, Result.Forbidden())
            : new ManagedStaff(user, Result.Success());
    }
}

public sealed record ManagedStaff(AppUser? User, Result Failure);
