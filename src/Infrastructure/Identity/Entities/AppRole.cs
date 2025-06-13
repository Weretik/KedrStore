namespace Infrastructure.Identity.Entities;

public class AppRole : IdentityRole<int>
{
    public string Description { get; set; } = string.Empty;
    public string Scope { get; set; }

    public bool IsSystemRole { get; set; } = false;
    public int AccessLevel { get; set; }

    public ICollection<AppUserRole> UserRoles { get; set; } = new List<AppUserRole>();
}
