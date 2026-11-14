using System.Text.Json;
using static Sales.Infrastructure.Orders.WorkspaceIdempotency;
using Sales.Application.Contracts.Orders;
using Sales.Application.Features.Orders.AdminWorkspace;

namespace Sales.Infrastructure.Orders;

/// <summary>Saves the manager's drafts; the input passed the workspace validators.</summary>
internal sealed class WorkspaceDraftWriter(SalesDbContext db, AdminWorkspacePricing pricing, IWorkspaceDraftReader reader, WorkspaceDraftDetails details) : IWorkspaceDraftWriter
{
    public async Task<DomainResult<(WorkspaceDraftDetail Draft, bool Replay)>> CreateDraftAsync(
        WorkspaceDraftInput request, Guid idempotencyKey, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var key = idempotencyKey.ToString("D");
        var hash = Hash(JsonSerializer.Serialize(new
        {
            title = request.Title.Trim(), note = request.Note?.Trim(), customer = request.CounterpartyId.Trim(),
            lines = request.Lines.Select(x => new { x.ProductId, x.Quantity, adjustment = x.PriceAdjustment.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) })
        }));
        var existing = await db.OrderDraftIdempotencyRecords.AsNoTracking()
            .SingleOrDefaultAsync(x => x.IdempotencyKey == key, ct);
        if (existing is not null)
            return await ReplayAsync(existing, hash, ct);

        var quote = await pricing.QuoteAsync(new WorkspaceQuoteRequest(request.CounterpartyId, request.Lines), ct);
        if (quote.IsFailure)
            return quote.Error!;
        var now = DateTimeOffset.UtcNow;
        var draft = OrderDraft.Create(request.Title, request.Note, quote.Value.CounterpartyId,
            request.Lines.Select(x => OrderDraftLine.Create(x.ProductId, x.Quantity, x.PriceAdjustment)), now);
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
            return await ReplayAsync(existing, hash, ct);
        }
        return (await details.ToDraftDetailAsync(draft, ct), false);
    }

    public async Task<DomainResult<WorkspaceDraftDetail>> UpdateDraftAsync(
        long draftId, string version, WorkspaceDraftInput request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var id = OrderDraftId.FromStorage(draftId);
        var draft = await db.OrderDrafts.Include(x => x.Lines).SingleOrDefaultAsync(x => x.Id == id, ct);
        if (draft is null)
            return WorkspaceErrors.DraftNotFound();
        if (!IsVersion(draft.Version, version))
            return WorkspaceErrors.VersionConflict();
        var quote = await pricing.QuoteAsync(new WorkspaceQuoteRequest(request.CounterpartyId, request.Lines), ct);
        if (quote.IsFailure)
            return quote.Error!;
        draft.Update(request.Title, request.Note, quote.Value.CounterpartyId,
            request.Lines.Select(x => OrderDraftLine.Create(x.ProductId, x.Quantity, x.PriceAdjustment)), DateTimeOffset.UtcNow);
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException)
        {
            return WorkspaceErrors.VersionConflict();
        }
        return await details.ToDraftDetailAsync(draft, ct);
    }

    public async Task<DomainResult> DeleteDraftAsync(long draftId, string version, CancellationToken ct)
    {
        var id = OrderDraftId.FromStorage(draftId);
        var draft = await db.OrderDrafts.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (draft is null)
            return WorkspaceErrors.DraftNotFound();
        if (!IsVersion(draft.Version, version))
            return WorkspaceErrors.VersionConflict();
        db.OrderDrafts.Remove(draft);
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException)
        {
            return WorkspaceErrors.VersionConflict();
        }
        return DomainResult.Success();
    }

    private async Task<DomainResult<(WorkspaceDraftDetail Draft, bool Replay)>> ReplayAsync(
        OrderDraftIdempotencyRecord existing, string hash, CancellationToken ct)
    {
        if (existing.RequestHash != hash)
            return WorkspaceErrors.IdempotencyConflict();
        var replay = await reader.GetDraftAsync(existing.DraftId.Value, ct);
        if (replay is null)
            return WorkspaceErrors.DraftDeleted();
        return (replay, true);
    }

    /// <summary>The <c>If-Match</c> value, with or without quotes, names the stored version.</summary>
    private static bool IsVersion(string actual, string? supplied) =>
        !string.IsNullOrWhiteSpace(supplied) &&
        string.Equals(actual, supplied.Trim().Trim('"'), StringComparison.Ordinal);
}
