namespace Identity.Infrastructure.Entities;

public class AppUser : IdentityUser<int>
{
    public string FullName { get; set; } = string.Empty;
    public ICollection<AppUserRole> UserRoles { get; set; } = new List<AppUserRole>();
}
