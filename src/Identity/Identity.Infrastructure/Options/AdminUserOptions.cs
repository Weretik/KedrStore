namespace Identity.Infrastructure.Options;

public class AdminUserOptions
{
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? DefaultPassword { get; set; } = string.Empty;
    public bool LockoutEnabled { get; set; }
}
