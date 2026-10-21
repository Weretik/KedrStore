using System.Text.Json;
using Sales.Application.Contracts.Orders;

namespace Sales.Infrastructure.Orders;

internal sealed partial class AdminOrderWorkspaceService
{
    public async Task<WorkspacePage<WorkspaceDraftListItem>> GetDraftsAsync(
        string? searchTerm, string? counterpartyId, string sort, int page, int pageSize, CancellationToken ct)
    {
        ValidatePage(searchTerm, counterpartyId, page, pageSize);
        var query =
            from draft in db.OrderDrafts.AsNoTracking()
            join customer in db.Counterparties.IgnoreQueryFilters().AsNoTracking()
                on draft.CounterpartyId equals customer.Id
            where counterpartyId == null || draft.CounterpartyId == counterpartyId
            select new { draft, customer };
        foreach (var term in Terms(searchTerm))
        {
            var pattern = "%" + EscapeLike(term) + "%";
            var numericId = long.TryParse(term, out var parsedId) ? parsedId : -1;
            query = query.Where(x => EF.Functions.ILike(x.draft.Title, pattern, "\\") ||
                                     EF.Functions.ILike(x.customer.Name, pattern, "\\") ||
                                     x.draft.Id == OrderDraftId.FromStorage(numericId));
        }
        var count = await query.LongCountAsync(ct);
        query = sort switch
        {
            "updatedAt" => query.OrderBy(x => x.draft.UpdatedAtUtc).ThenBy(x => x.draft.Id),
            "-updatedAt" => query.OrderByDescending(x => x.draft.UpdatedAtUtc).ThenByDescending(x => x.draft.Id),
            "title" => query.OrderBy(x => x.draft.Title).ThenBy(x => x.draft.Id),
            "-title" => query.OrderByDescending(x => x.draft.Title).ThenByDescending(x => x.draft.Id),
            "customerName" => query.OrderBy(x => x.customer.Name).ThenBy(x => x.draft.Id),
            "-customerName" => query.OrderByDescending(x => x.customer.Name).ThenByDescending(x => x.draft.Id),
            _ => throw new WorkspaceException(400, "invalid_sort", "Unsupported draft sort.")
        };
        var rows = await query.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new WorkspaceDraftListItem(x.draft.Id.Value, x.draft.Title,
                x.draft.CounterpartyId, x.customer.Name, x.draft.UpdatedAtUtc,
                x.draft.Lines.Count)).ToListAsync(ct);
        return new WorkspacePage<WorkspaceDraftListItem>(PageInfo(page, pageSize, count), rows);
    }

    public async Task<WorkspaceDraftDetail?> GetDraftAsync(long draftId, CancellationToken ct)
    {
        if (draftId < 1) return null;
        var id = OrderDraftId.FromStorage(draftId);
        var draft = await db.OrderDrafts.AsNoTracking().Include(x => x.Lines)
            .SingleOrDefaultAsync(x => x.Id == id, ct);
        if (draft is null) return null;
        return await ToDraftDetailAsync(draft, ct);
    }

    public async Task<(WorkspaceDraftDetail Draft, bool Replay)> CreateDraftAsync(
        WorkspaceDraftInput request, Guid idempotencyKey, CancellationToken ct)
    {
        if (idempotencyKey == Guid.Empty)
            throw new WorkspaceException(400, "invalid_idempotency_key", "Idempotency-Key must be a UUID.");
        ValidateDraftFields(request);
        var key = idempotencyKey.ToString("D");
        var hash = Hash(JsonSerializer.Serialize(new
        {
            title = request.Title.Trim(), note = request.Note?.Trim(), customer = request.CounterpartyId.Trim(),
            lines = request.Lines.Select(x => new { x.ProductId, x.Quantity })
        }));
        var existing = await db.OrderDraftIdempotencyRecords.AsNoTracking()
            .SingleOrDefaultAsync(x => x.IdempotencyKey == key, ct);
        if (existing is not null)
        {
            if (existing.RequestHash != hash)
                throw new WorkspaceException(409, "idempotency_conflict", "Idempotency-Key has a different request.");
            var replay = await GetDraftAsync(existing.DraftId.Value, ct);
            if (replay is null)
                throw new WorkspaceException(409, "draft_deleted", "The original draft was deleted.");
            return (replay, true);
        }

        var quote = await pricing.QuoteAsync(new WorkspaceQuoteRequest(request.CounterpartyId, request.Lines), ct);
        var now = DateTimeOffset.UtcNow;
        var draft = OrderDraft.Create(request.Title, request.Note, quote.CounterpartyId,
            request.Lines.Select(x => OrderDraftLine.Create(x.ProductId, x.Quantity)), now);
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        db.OrderDrafts.Add(draft);
        await db.SaveChangesAsync(ct);
        db.OrderDraftIdempotencyRecords.Add(new OrderDraftIdempotencyRecord
        {
            IdempotencyKey = key, RequestHash = hash, DraftId = draft.Id, CreatedAtUtc = now
        });
        try
        {
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch (DbUpdateException error) when (IsUniqueViolation(error))
        {
            await transaction.RollbackAsync(ct);
            existing = await db.OrderDraftIdempotencyRecords.AsNoTracking()
                .SingleOrDefaultAsync(x => x.IdempotencyKey == key, ct);
            if (existing is null) throw;
            if (existing.RequestHash != hash)
                throw new WorkspaceException(409, "idempotency_conflict", "Idempotency-Key has a different request.");
            var replay = await GetDraftAsync(existing.DraftId.Value, ct);
            if (replay is null)
                throw new WorkspaceException(409, "draft_deleted", "The original draft was deleted.");
            return (replay, true);
        }
        return (await ToDraftDetailAsync(draft, ct), false);
    }

    public async Task<WorkspaceDraftDetail?> UpdateDraftAsync(
        long draftId, string version, WorkspaceDraftInput request, CancellationToken ct)
    {
        if (draftId < 1) return null;

        var id = OrderDraftId.FromStorage(draftId);
        var draft = await db.OrderDrafts.Include(x => x.Lines).SingleOrDefaultAsync(x => x.Id == id, ct);
        if (draft is null) return null;
        CheckVersion(draft.Version, version);
        var quote = await ValidateDraftAsync(request, ct);
        draft.Update(request.Title, request.Note, quote.CounterpartyId,
            request.Lines.Select(x => OrderDraftLine.Create(x.ProductId, x.Quantity)), DateTimeOffset.UtcNow);
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException)
        {
            throw new WorkspaceException(409, "version_conflict", "Draft has changed.");
        }
        return await ToDraftDetailAsync(draft, ct);
    }

    public async Task<bool> DeleteDraftAsync(long draftId, string version, CancellationToken ct)
    {
        if (draftId < 1) return false;
        var id = OrderDraftId.FromStorage(draftId);
        var draft = await db.OrderDrafts.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (draft is null) return false;
        CheckVersion(draft.Version, version);
        db.OrderDrafts.Remove(draft);
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException)
        {
            throw new WorkspaceException(409, "version_conflict", "Draft has changed.");
        }
        return true;
    }

    private static void ValidateDraftFields(WorkspaceDraftInput request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Title) || request.Title.Trim().Length > 200 ||
            request.Note?.Length > 2000)
            throw new WorkspaceException(400, "invalid_draft", "Draft title or note is invalid.");
        AdminWorkspacePricing.ValidateRequest(new WorkspaceQuoteRequest(request.CounterpartyId, request.Lines));
    }

    private async Task<WorkspaceQuote> ValidateDraftAsync(WorkspaceDraftInput request, CancellationToken ct)
    {
        ValidateDraftFields(request);
        return await pricing.QuoteAsync(new WorkspaceQuoteRequest(request.CounterpartyId, request.Lines), ct);
    }

    private async Task<WorkspaceDraftDetail> ToDraftDetailAsync(OrderDraft draft, CancellationToken ct)
    {
        var customer = await db.Counterparties.IgnoreQueryFilters().AsNoTracking()
            .Where(x => x.Id == draft.CounterpartyId).Select(x => x.Name).SingleAsync(ct);
        try
        {
            var quote = await pricing.QuoteAsync(new WorkspaceQuoteRequest(
                draft.CounterpartyId, draft.Lines.Select(x => new WorkspaceInputLine(x.ProductId, x.Quantity)).ToArray()), ct);
            return new WorkspaceDraftDetail(draft.Id.Value, draft.Version, draft.Title, draft.Note,
                draft.CounterpartyId, customer, draft.CreatedAtUtc, draft.UpdatedAtUtc,
                quote.Lines.Select(x => new WorkspaceDraftQuotedLine(
                    x.ProductId, x.ProductName, x.Quantity, x.UnitPrice, x.LineAmount)).ToArray(),
                quote.TotalAmount, "Available", null);
        }
        catch (WorkspaceException error) when (error.Code is "customer_not_found" or "product_not_found" or
                   "unavailable_product" or "unavailable_price" or "amount_overflow" or "invalid_lines")
        {
            var lines = await pricing.DescribeDraftLinesAsync(draft.Lines, ct);
            return new WorkspaceDraftDetail(draft.Id.Value, draft.Version, draft.Title, draft.Note,
                draft.CounterpartyId, customer, draft.CreatedAtUtc, draft.UpdatedAtUtc,
                lines, null, "Unavailable", error.Code);
        }
    }

    private static void CheckVersion(string actual, string supplied)
    {
        if (string.IsNullOrWhiteSpace(supplied) ||
            !string.Equals(actual, supplied.Trim().Trim('"'), StringComparison.Ordinal))
            throw new WorkspaceException(409, "version_conflict", "Draft has changed.");
    }
}
