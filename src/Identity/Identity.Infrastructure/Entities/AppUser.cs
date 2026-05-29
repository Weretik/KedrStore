namespace Identity.Infrastructure.Entities;

public class AppUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
    public ICollection<AppUserRole> UserRoles { get; set; } = new List<AppUserRole>();
}
