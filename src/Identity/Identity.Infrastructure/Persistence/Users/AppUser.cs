namespace Identity.Infrastructure.Persistence.Users;

public class AppUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;

    /// <summary>Avatar file key relative to the CDN avatars folder; null when the user has none.</summary>
    public string? AvatarPath { get; set; }
    public ICollection<AppUserRole> UserRoles { get; set; } = new List<AppUserRole>();
}
