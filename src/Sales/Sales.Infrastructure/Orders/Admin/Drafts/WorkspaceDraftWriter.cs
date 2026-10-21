using System.Text.Json;
using static Sales.Infrastructure.Orders.WorkspaceIdempotency;
using Sales.Application.Contracts.Orders;

namespace Sales.Infrastructure.Orders;

internal sealed class WorkspaceDraftWriter(SalesDbContext db, AdminWorkspacePricing pricing, IWorkspaceDraftReader reader, WorkspaceDraftDetails details) : IWorkspaceDraftWriter
{
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
            var replay = await reader.GetDraftAsync(existing.DraftId.Value, ct);
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
            var replay = await reader.GetDraftAsync(existing.DraftId.Value, ct);
            if (replay is null)
                throw new WorkspaceException(409, "draft_deleted", "The original draft was deleted.");
            return (replay, true);
        }
        return (await details.ToDraftDetailAsync(draft, ct), false);
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
        return await details.ToDraftDetailAsync(draft, ct);
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

    private static void CheckVersion(string actual, string supplied)
    {
        if (string.IsNullOrWhiteSpace(supplied) ||
            !string.Equals(actual, supplied.Trim().Trim('"'), StringComparison.Ordinal))
            throw new WorkspaceException(409, "version_conflict", "Draft has changed.");
    }

}
