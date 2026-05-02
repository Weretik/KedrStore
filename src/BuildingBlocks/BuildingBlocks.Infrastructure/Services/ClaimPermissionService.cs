using System.Security.Claims;
using BuildingBlocks.Application.Contracts;
using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Infrastructure.Services;

public sealed class ClaimPermissionService(IHttpContextAccessor httpContextAccessor) : IPermissionService
{
    public Task<bool> HasPermissionAsync(int userId, string permission, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var principal = httpContextAccessor.HttpContext?.User;
        if (principal?.Identity?.IsAuthenticated != true || string.IsNullOrWhiteSpace(permission))
        {
            return Task.FromResult(false);
        }

        if (principal.IsInRole("Admin"))
        {
            return Task.FromResult(true);
        }

        var currentUserIdRaw = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(currentUserIdRaw, out var currentUserId) || currentUserId != userId)
        {
            return Task.FromResult(false);
        }

        var hasPermission = principal.Claims.Any(claim =>
            string.Equals(claim.Type, "permission", StringComparison.OrdinalIgnoreCase) &&
            string.Equals(claim.Value, permission, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(hasPermission);
    }
}
