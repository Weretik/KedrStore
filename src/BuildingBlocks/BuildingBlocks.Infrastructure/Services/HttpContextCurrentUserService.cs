using System.Security.Claims;
using BuildingBlocks.Application.Contracts;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Infrastructure.Services;

public sealed class HttpContextCurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    public string UserId => GetClaimValue(ClaimTypes.NameIdentifier);

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public string UserName =>
        GetClaimValue(ClaimTypes.Name) is { Length: > 0 } name
            ? name
            : GetClaimValue("name");

    public string Email =>
        GetClaimValue(ClaimTypes.Email) is { Length: > 0 } email
            ? email
            : GetClaimValue("email");

    public IEnumerable<string> Roles =>
        Principal?.FindAll(ClaimTypes.Role).Select(x => x.Value) ?? [];

    public IEnumerable<Claim> Claims => Principal?.Claims ?? [];

    public bool IsInRole(string role) =>
        Principal?.IsInRole(role) ?? false;

    public bool HasPermission(string permission)
    {
        if (string.IsNullOrWhiteSpace(permission) || Principal is null)
        {
            return false;
        }

        return Principal.Claims.Any(claim =>
            string.Equals(claim.Type, "permission", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(claim.Value, permission, StringComparison.OrdinalIgnoreCase));
    }

    public string GetClaimValue(string claimType)
    {
        if (string.IsNullOrWhiteSpace(claimType))
        {
            return string.Empty;
        }

        return Principal?.FindFirst(claimType)?.Value ?? string.Empty;
    }

    private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;
}
