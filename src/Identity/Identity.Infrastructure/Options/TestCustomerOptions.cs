namespace Identity.Infrastructure.Options;

public class TestCustomerOptions
{
    public Guid IdentityUserId { get; set; }
    public string CounterpartyId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string DefaultPassword { get; set; } = string.Empty;
    public bool LockoutEnabled { get; set; }
}
