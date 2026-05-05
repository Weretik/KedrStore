namespace Identity.Infrastructure.Entities;

public class AppUserRole : IdentityUserRole<int>
{
    public AppUser User { get; set; } = null!;
    public AppRole Role { get; set; } = null!;

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    public string AssignedBy { get; set; } = string.Empty;
    public bool IsTemporary { get; set; }
    public DateTime? ExpiresAt { get; set; }

    public string Notes { get; set; } = string.Empty;
}
