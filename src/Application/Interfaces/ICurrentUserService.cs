namespace Application.Interfaces
{
    public interface ICurrentUserService
    {
        string UserId { get; }
        bool IsAuthenticated { get; }
        string UserName { get; }
        string Email { get; }
        IEnumerable<string> Roles { get; }
        IEnumerable<Claim> Claims { get; }
        bool IsInRole(string role);
        bool HasPermission(string permission);
        string GetClaimValue(string claimType);
    }
}
