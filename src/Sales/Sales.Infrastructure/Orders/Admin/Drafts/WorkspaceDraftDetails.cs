using Sales.Application.Abstractions.Orders;

namespace Sales.Infrastructure.Orders;

internal sealed class WorkspaceDraftDetails(SalesDbContext db, AdminWorkspacePricing pricing)
{
    internal async Task<WorkspaceDraftDetail> ToDraftDetailAsync(OrderDraft draft, CancellationToken ct)
    {
        var customer = await db.Counterparties.IgnoreQueryFilters().AsNoTracking()
            .Where(x => x.Id == draft.CounterpartyId).Select(x => x.Name).SingleAsync(ct);
        var quote = await pricing.QuoteAsync(new WorkspaceQuoteRequest(
            draft.CounterpartyId, draft.Lines.Select(x => new WorkspaceInputLine(x.ProductId, x.Quantity, x.PriceAdjustment)).ToArray()), ct);
        if (quote.IsSuccess)
        {
            return new WorkspaceDraftDetail(draft.Id.Value, draft.Version, draft.Title, draft.Note,
                draft.CounterpartyId, customer, draft.CreatedAtUtc, draft.UpdatedAtUtc,
                quote.Value.Lines.Select(x => new WorkspaceDraftQuotedLine(
                    x.ProductId, x.ProductName, x.Quantity, x.UnitPrice, x.LineAmount, x.PriceAdjustment, x.BaseUnitPrice)).ToArray(),
                quote.Value.TotalAmount, "Available", null);
        }

        // A saved draft stays readable when it can no longer be priced; the code says why.
        var lines = await pricing.DescribeDraftLinesAsync(draft.Lines, ct);
        return new WorkspaceDraftDetail(draft.Id.Value, draft.Version, draft.Title, draft.Note,
            draft.CounterpartyId, customer, draft.CreatedAtUtc, draft.UpdatedAtUtc,
            lines, null, "Unavailable", quote.Error!.Code);
    }
}
