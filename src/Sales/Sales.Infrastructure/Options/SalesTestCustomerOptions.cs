namespace Sales.Infrastructure.Options;

public sealed class SalesTestCustomerOptions
{
    public const string SectionName = "Identity:TestCustomer";

    public Guid IdentityUserId { get; init; }
    public string CounterpartyId { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
}
