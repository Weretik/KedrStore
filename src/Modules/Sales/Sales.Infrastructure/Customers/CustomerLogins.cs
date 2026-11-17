using Identity.Contracts.Users;
using Sales.Application.Abstractions.Customers;

namespace Sales.Infrastructure.Customers;

internal sealed class CustomerLogins(IImportedCounterpartyIdentityProvisioningService identity) : ICustomerLogins
{
    public Task<Result<Guid>> EnsureAsync(
        string counterpartyId,
        string email,
        string name,
        Guid? existingUserId,
        CancellationToken cancellationToken)
        => identity.EnsureUserAsync(
            new ImportedCounterpartyIdentityUserData(counterpartyId, email, name),
            existingUserId,
            cancellationToken);
}
