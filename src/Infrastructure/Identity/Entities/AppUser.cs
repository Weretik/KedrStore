using Domain.Identity.ValueObjects;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity.Entities;

public class AppUser : IdentityUser<int>
{
    public string FullName { get; set; } = string.Empty;
    public virtual ICollection<AppUserRole> UserRoles { get; set; } = new List<AppUserRole>();
}
