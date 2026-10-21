using System.Text.Json;
using Sales.Application.Contracts.Persistence;
using Sales.Domain.Orders.Enums;
using static Sales.Infrastructure.Orders.WorkspaceIdempotency;
using Sales.Application.Contracts.Orders;

namespace Sales.Infrastructure.Orders;

internal sealed class WorkspaceOrderCreator(SalesDbContext db, AdminWorkspacePricing pricing, IOrderCreationStore orderStore, IOrderNumberGenerator numberGenerator) : IWorkspaceOrderCreator
{
    private const string OrderOperation = "create-admin-workspace-order";

    public async Task<(WorkspaceCreatedOrder Order, bool Replay, WorkspaceOrderConflict? Conflict)> CreateOrderAsync(
        WorkspaceCreateOrderRequest request, Guid idempotencyKey, CancellationToken ct)
    {
        if (request is null || idempotencyKey == Guid.Empty || string.IsNullOrWhiteSpace(request.QuoteFingerprint) ||
            request.Comment?.Length > 1000)
            throw new WorkspaceException(400, "invalid_request", "Idempotency key, quote fingerprint or comment is invalid.");
        AdminWorkspacePricing.ValidateRequest(new WorkspaceQuoteRequest(request.CounterpartyId, request.Lines));
        var hash = Hash(JsonSerializer.Serialize(new
        {
            customer = request.CounterpartyId?.Trim(),
            lines = request.Lines?.Select(x => new { product = x.ProductId.Trim(), x.Quantity }).ToArray(),
            fingerprint = request.QuoteFingerprint,
            comment = request.Comment?.Trim()
        }));
        var key = idempotencyKey.ToString("D");
        var existing = await orderStore.FindIdempotencyRecordAsync(OrderOperation, key, ct);
        if (existing is not null)
        {
            if (existing.RequestHash != hash)
                return (null!, false, new WorkspaceOrderConflict("idempotency_conflict", null));
            var total = await db.OrderLines.AsNoTracking()
                .Where(x => EF.Property<OrderId>(x, "OrderId") == existing.OrderId)
                .SumAsync(x => x.Amount, ct);
            return (new WorkspaceCreatedOrder(existing.OrderId.Value, existing.OrderNumber, total,
                existing.SyncStatus.ToString()), true, null);
        }

        var quote = await pricing.QuoteAsync(new WorkspaceQuoteRequest(request.CounterpartyId!, request.Lines!), ct);
        if (!string.Equals(quote.QuoteFingerprint, request.QuoteFingerprint, StringComparison.Ordinal))
            return (null!, false, new WorkspaceOrderConflict("quote_changed", quote));
        var now = DateTimeOffset.UtcNow;
        var order = Order.Create(await numberGenerator.GenerateAsync(now, ct), quote.CounterpartyId,
            request.Comment, quote.Lines.Select(x => OrderLine.Create(x.ProductId, x.ProductName, x.Quantity, x.LineAmount)), now);
        try
        {
            await orderStore.PersistAsync(order,
            id => OneCOrderSync.Create(id, now),
            id => new CreateOrderIdempotencyRecord(OrderOperation, key, hash, id,
                order.OrderNumber, OneCOrderSyncStatus.Pending, now, now.AddHours(24)), ct);
        }
        catch (DbUpdateException error) when (IsUniqueViolation(error))
        {
            existing = await orderStore.FindIdempotencyRecordAsync(OrderOperation, key, ct);
            if (existing is null) throw;
            if (existing.RequestHash != hash)
                return (null!, false, new WorkspaceOrderConflict("idempotency_conflict", null));
            var replayTotal = await db.OrderLines.AsNoTracking()
                .Where(x => EF.Property<OrderId>(x, "OrderId") == existing.OrderId)
                .SumAsync(x => x.Amount, ct);
            return (new WorkspaceCreatedOrder(existing.OrderId.Value, existing.OrderNumber,
                replayTotal, existing.SyncStatus.ToString()), true, null);
        }
        return (new WorkspaceCreatedOrder(order.Id.Value, order.OrderNumber, quote.TotalAmount, "Pending"), false, null);
    }
}
