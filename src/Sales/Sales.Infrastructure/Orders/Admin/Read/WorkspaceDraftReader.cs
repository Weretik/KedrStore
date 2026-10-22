using static Sales.Infrastructure.Orders.WorkspaceQueryTools;
using Sales.Application.Contracts.Orders;

namespace Sales.Infrastructure.Orders;

internal sealed class WorkspaceDraftReader(SalesDbContext db, WorkspaceDraftDetails details) : IWorkspaceDraftReader
{
    public async Task<WorkspacePage<WorkspaceDraftListItem>> GetDraftsAsync(
        string? searchTerm, string? counterpartyId,
        DateTimeOffset? createdFromUtc, DateTimeOffset? createdToUtcExclusive,
        string sort, int page, int pageSize, CancellationToken ct)
    {
        ValidatePage(searchTerm, counterpartyId, page, pageSize);
        var query =
            from draft in db.OrderDrafts.AsNoTracking()
            join customer in db.Counterparties.IgnoreQueryFilters().AsNoTracking()
                on draft.CounterpartyId equals customer.Id
            where counterpartyId == null || draft.CounterpartyId == counterpartyId
            select new { draft, customer };
        if (createdFromUtc is { } fromUtc)
            query = query.Where(x => x.draft.CreatedAtUtc >= fromUtc);
        if (createdToUtcExclusive is { } toUtc)
            query = query.Where(x => x.draft.CreatedAtUtc < toUtc);
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
        return await details.ToDraftDetailAsync(draft, ct);
    }
}
