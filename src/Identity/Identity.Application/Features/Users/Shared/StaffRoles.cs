using Identity.Contracts.Authorization;

namespace Identity.Application.Features.Users.Shared;

/// <summary>Roles an administrator can give to a staff account, strongest first.</summary>
public static class StaffRoles
{
    public static readonly IReadOnlyList<string> All = [RoleNames.Admin, RoleNames.Seo, RoleNames.Manager];

    public static bool IsStaffRole(string? role) => role is not null && All.Contains(role, StringComparer.Ordinal);
}
