using Identity.Domain.Authorization;

namespace Identity.Application.Features.Users.Shared;

/// <summary>Roles an administrator can give to a staff account.</summary>
public static class StaffRoles
{
    public static readonly IReadOnlyList<string> All = [RoleNames.Admin, RoleNames.Manager];

    public static bool IsStaffRole(string? role) => role is not null && All.Contains(role, StringComparer.Ordinal);
}
