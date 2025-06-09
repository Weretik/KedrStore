using Domain.Identity.ValueObjects;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity.Entities;


public class AppUserRole : IdentityUserRole<int>
{
    public virtual AppUser User { get; set; } = null!;
    public virtual AppRole Role { get; set; } = null!;

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public string AssignedByUserId { get; set; } = string.Empty;
    public bool IsTemporary { get; set; }
    public DateTime? ExpiresAt { get; set; }

    public string Notes { get; set; } = string.Empty;
}
