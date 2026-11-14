using System.Text.Json;
using Sales.Application.Abstractions.Persistence;
using Sales.Domain.Orders;
using static Sales.Infrastructure.Orders.WorkspaceIdempotency;
using Sales.Application.Abstractions.Orders;
using Sales.Application.Abstractions.Workspace.Orders;
using Sales.Application.Abstractions.Workspace.Pricing;
using Sales.Domain.Orders.ValueObjects;

namespace Sales.Infrastructure.Orders;

/// <summary>Turns a confirmed quote into an order; the input passed the workspace validator.</summary>
internal sealed class WorkspaceOrderCreator(SalesDbContext db, AdminWorkspacePricing pricing, IOrderCreationStore orderStore, IOrderNumberGenerator numberGenerator) : IWorkspaceOrderCreator
{
    private const string OrderOperation = "create-admin-workspace-order";

    public async Task<DomainResult<(WorkspaceCreatedOrder Order, bool Replay, WorkspaceOrderConflict? Conflict)>> CreateOrderAsync(
        WorkspaceCreateOrderRequest request, Guid idempotencyKey, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var hash = Hash(JsonSerializer.Serialize(new
        {
            customer = request.CounterpartyId?.Trim(),
            lines = request.Lines?.Select(x => new { product = x.ProductId.Trim(), x.Quantity, adjustment = x.PriceAdjustment.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) }).ToArray(),
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

        var quoted = await pricing.QuoteAsync(new WorkspaceQuoteRequest(request.CounterpartyId, request.Lines), ct);
        if (quoted.IsFailure)
            return quoted.Error!;
        var quote = quoted.Value;
        if (!string.Equals(quote.QuoteFingerprint, request.QuoteFingerprint, StringComparison.Ordinal))
            return (null!, false, new WorkspaceOrderConflict("quote_changed", quote));
        var now = DateTimeOffset.UtcNow;
        var lines = new List<OrderLine>(quote.Lines.Count);
        foreach (var line in quote.Lines)
        {
            var orderLine = OrderLine.Create(line.ProductId, line.ProductName, line.Quantity, line.LineAmount, line.PriceAdjustment);
            if (orderLine.IsFailure)
                return orderLine.Error!;
            lines.Add(orderLine.Value);
        }
        var created = Order.Create(await numberGenerator.GenerateAsync(now, ct), quote.CounterpartyId,
            request.Comment, lines, now);
        if (created.IsFailure)
            return created.Error!;
        var order = created.Value;
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
