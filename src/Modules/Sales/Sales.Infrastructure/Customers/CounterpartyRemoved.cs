using BuildingBlocks.Application.Outbox;

namespace Sales.Infrastructure.Customers;

/// <summary>
/// A customer 1C no longer has was removed from Sales; its Identity user is deleted by the outbox (platform spec 006,
/// R-014). Stored in the transaction that removes the customer.
/// </summary>
public sealed record CounterpartyRemoved(string CounterpartyId, Guid IdentityUserId) : IOutboxMessage
{
    /// <summary>The stored type name; keep it when the record is renamed.</summary>
    public const string TypeName = "sales.counterparty-removed";
}
