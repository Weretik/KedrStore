using System.Security.Claims;
using BuildingBlocks.Application.Users;

namespace IntegrationTests.TestSupport.Content;

/// <summary>A signed-in manager for handlers that record who saved a change.</summary>
public sealed class TestContentEditor : ICurrentUserService
{
    public const string EmailAddress = "manager@classkedr.com.ua";

    public string UserId => "7";
    public bool IsAuthenticated => true;
    public string UserName => "manager";
    public string Email => EmailAddress;
    public IEnumerable<string> Roles => ["Manager"];
    public IEnumerable<Claim> Claims => [];
    public bool IsInRole(string role) => role == "Manager";
    public bool HasPermission(string permission) => false;
    public string GetClaimValue(string claimType) => string.Empty;
}
