namespace Sales.Application.Abstractions.Customers;

/// <summary>The site login of an imported customer, kept by Identity.</summary>
public interface ICustomerLogins
{
    /// <summary>Creates the login or updates the linked one; answers its user id.</summary>
    Task<Result<Guid>> EnsureAsync(
        string counterpartyId,
        string email,
        string name,
        Guid? existingUserId,
        CancellationToken cancellationToken);
}
