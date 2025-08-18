using Application.Common.Interfaces;

namespace Infrastructure.Identity.Security;

public class FakeCurrentUserService : ICurrentUserService
{
    public string UserId { get; init; } = "fake-user-id";
    public bool IsAuthenticated { get; init; } = true;
    public string UserName { get; init; } = "FakeUser";
    public string Email { get; init; } = "fakeuser@example.com";

    public IEnumerable<string> Roles { get; init; } = new[] { "Admin", "User" };

    public IEnumerable<Claim> Claims { get; init; } = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, "fake-user-id"),
        new(ClaimTypes.Name, "FakeUser"),
        new(ClaimTypes.Email, "fakeuser@example.com"),
        new(ClaimTypes.Role, "Admin")
    };

    public bool IsInRole(string role) =>
        Roles.Contains(role, StringComparer.OrdinalIgnoreCase);

    public bool HasPermission(string permission) =>
        // Настраивай по необходимости
        permission switch
        {
            "ViewProducts" => true,
            "EditProducts" => true,
            _ => false
        };

    public string GetClaimValue(string claimType) =>
        Claims.FirstOrDefault(c => c.Type == claimType)?.Value ?? string.Empty;
}
