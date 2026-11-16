using BuildingBlocks.Application.Outbox;
using Identity.Contracts.Users;

namespace Sales.Infrastructure.Customers;

/// <summary>
/// Deletes the Identity user of a removed customer through <c>Identity.Contracts</c> (platform spec 006, R-014). A user
/// another customer still uses is kept; a user already gone counts as deleted.
/// </summary>
internal sealed class RemovedCounterpartyIdentityDeleter(
    SalesDbContext db,
    IImportedCounterpartyIdentityProvisioningService identity) : IOutboxMessageHandler<CounterpartyRemoved>
{
    public async Task<OutboxHandlingResult> HandleAsync(
        CounterpartyRemoved message,
        Guid messageId,
        CancellationToken cancellationToken)
    {
        if (await db.Counterparties.IgnoreQueryFilters().AsNoTracking()
                .AnyAsync(customer => customer.IdentityUserId == message.IdentityUserId, cancellationToken))
            return OutboxHandlingResult.Delivered;

        var deleted = await identity.DeleteUserAsync(message.IdentityUserId, message.CounterpartyId, cancellationToken);
        return deleted.IsSuccess
            ? OutboxHandlingResult.Delivered
            : OutboxHandlingResult.Failed(string.Join(", ", deleted.Errors));
    }
}
