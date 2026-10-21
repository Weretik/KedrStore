using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Sales.Application.Contracts.Orders;
using Sales.Application.Contracts.Persistence;
using Sales.Domain.Orders.Enums;
using Npgsql;

namespace Sales.Infrastructure.Orders;

internal sealed partial class AdminOrderWorkspaceService(
    SalesDbContext db,
    AdminWorkspacePricing pricing,
    IOrderCreationStore orderStore,
    IOrderNumberGenerator numberGenerator) : IAdminOrderWorkspaceService
{
    private const string OrderOperation = "create-admin-workspace-order";

    public Task<WorkspaceQuote> QuoteAsync(WorkspaceQuoteRequest request, CancellationToken ct)
        => pricing.QuoteAsync(request, ct);

    public async Task<WorkspacePage<WorkspaceOrderListItem>> GetOrdersAsync(
        string? searchTerm, string? counterpartyId, string sort, int page, int pageSize, CancellationToken ct)
    {
        ValidatePage(searchTerm, counterpartyId, page, pageSize);
        var query =
            from order in db.Orders.AsNoTracking()
            join customer in db.Counterparties.IgnoreQueryFilters().AsNoTracking() on order.CounterpartyId equals customer.Id
            join sync in db.OneCOrderSyncs.AsNoTracking() on order.Id equals sync.OrderId
            where counterpartyId == null || order.CounterpartyId == counterpartyId
            select new { order, customer, sync };
        foreach (var term in Terms(searchTerm))
        {
            var pattern = "%" + EscapeLike(term) + "%";
            var numericId = long.TryParse(term, out var parsedId) ? parsedId : -1;
            query = query.Where(x => EF.Functions.ILike(x.order.OrderNumber, pattern, "\\") ||
                                     EF.Functions.ILike(x.customer.Name, pattern, "\\") ||
                                     x.order.Id == OrderId.FromStorage(numericId));
        }
        var count = await query.LongCountAsync(ct);
        query = sort switch
        {
            "createdAt" => query.OrderBy(x => x.order.CreatedAt).ThenBy(x => x.order.Id),
            "-createdAt" => query.OrderByDescending(x => x.order.CreatedAt).ThenByDescending(x => x.order.Id),
            "orderNumber" => query.OrderBy(x => x.order.OrderNumber).ThenBy(x => x.order.Id),
            "-orderNumber" => query.OrderByDescending(x => x.order.OrderNumber).ThenByDescending(x => x.order.Id),
            "customerName" => query.OrderBy(x => x.customer.Name).ThenBy(x => x.order.Id),
            "-customerName" => query.OrderByDescending(x => x.customer.Name).ThenByDescending(x => x.order.Id),
            "totalAmount" => query.OrderBy(x => x.order.Lines.Sum(y => y.Amount)).ThenBy(x => x.order.Id),
            "-totalAmount" => query.OrderByDescending(x => x.order.Lines.Sum(y => y.Amount)).ThenByDescending(x => x.order.Id),
            _ => throw new WorkspaceException(400, "invalid_sort", "Unsupported order sort.")
        };
        var rows = await query.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new WorkspaceOrderListItem(x.order.Id.Value, x.order.OrderNumber,
                x.order.CounterpartyId, x.customer.Name, x.order.CreatedAt,
                x.order.Lines.Count, x.order.Lines.Sum(y => y.Amount), x.sync.Status.ToString()))
            .ToListAsync(ct);
        return new WorkspacePage<WorkspaceOrderListItem>(PageInfo(page, pageSize, count), rows);
    }

    public async Task<WorkspaceOrderDetail?> GetOrderAsync(long orderId, CancellationToken ct)
    {
        if (orderId < 1) return null;
        var id = OrderId.FromStorage(orderId);
        var row = await (
            from order in db.Orders.AsNoTracking()
            join customer in db.Counterparties.IgnoreQueryFilters().AsNoTracking() on order.CounterpartyId equals customer.Id
            join sync in db.OneCOrderSyncs.AsNoTracking() on order.Id equals sync.OrderId
            where order.Id == id
            select new { order, customer, sync }).SingleOrDefaultAsync(ct);
        if (row is null) return null;
        var lines = row.order.Lines.OrderBy(x => x.Id.Value)
            .Select(x => new WorkspaceStoredLine(x.ProductId, x.ProductName, x.Quantity, x.Amount)).ToArray();
        // Lines are explicitly loaded below because this read is detached.
        var stored = await db.OrderLines.AsNoTracking()
            .Where(x => EF.Property<OrderId>(x, "OrderId") == id)
            .OrderBy(x => x.Id).Select(x => new WorkspaceStoredLine(x.ProductId, x.ProductName, x.Quantity, x.Amount))
            .ToArrayAsync(ct);
        return new WorkspaceOrderDetail(orderId, row.order.OrderNumber, row.order.CounterpartyId,
            row.customer.Name, row.order.CreatedAt, row.order.Comment, stored,
            stored.Sum(x => x.Amount), row.sync.Status.ToString(), row.sync.OneCDocumentNumber);
    }

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

    private static bool IsUniqueViolation(DbUpdateException error)
        => error.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };

    private static string Hash(string text)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)));

    private static WorkspacePagedInfo PageInfo(int page, int pageSize, long count)
        => new(page, pageSize, (count + pageSize - 1) / pageSize, count);

    private static void ValidatePage(string? search, string? customer, int page, int size)
    {
        if (page < 1 || size is < 1 or > 100 || search?.Length > 100 || customer?.Length > 64)
            throw new WorkspaceException(400, "invalid_query", "Page, search or customer filter is invalid.");
    }

    private static IEnumerable<string> Terms(string? search)
        => (search ?? "").Split((char[]?)null, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

    private static string EscapeLike(string term)
        => term.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}
