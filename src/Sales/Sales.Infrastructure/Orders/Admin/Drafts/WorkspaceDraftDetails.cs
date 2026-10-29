using Sales.Application.Contracts.Orders;

namespace Sales.Infrastructure.Orders;

internal sealed class WorkspaceDraftDetails(SalesDbContext db, AdminWorkspacePricing pricing)
{
    internal async Task<WorkspaceDraftDetail> ToDraftDetailAsync(OrderDraft draft, CancellationToken ct)
    {
        var customer = await db.Counterparties.IgnoreQueryFilters().AsNoTracking()
            .Where(x => x.Id == draft.CounterpartyId).Select(x => x.Name).SingleAsync(ct);
        try
        {
            var quote = await pricing.QuoteAsync(new WorkspaceQuoteRequest(
                draft.CounterpartyId, draft.Lines.Select(x => new WorkspaceInputLine(x.ProductId, x.Quantity, x.PriceAdjustment)).ToArray()), ct);
            return new WorkspaceDraftDetail(draft.Id.Value, draft.Version, draft.Title, draft.Note,
                draft.CounterpartyId, customer, draft.CreatedAtUtc, draft.UpdatedAtUtc,
                quote.Lines.Select(x => new WorkspaceDraftQuotedLine(
                    x.ProductId, x.ProductName, x.Quantity, x.UnitPrice, x.LineAmount, x.PriceAdjustment, x.BaseUnitPrice)).ToArray(),
                quote.TotalAmount, "Available", null);
        }
        catch (WorkspaceException error) when (error.Code is "customer_not_found" or "product_not_found" or
                   "unavailable_product" or "unavailable_price" or "price_below_minimum" or "amount_overflow" or "invalid_lines" or
                   "invalid_price_adjustment")
        {
            var lines = await pricing.DescribeDraftLinesAsync(draft.Lines, ct);
            return new WorkspaceDraftDetail(draft.Id.Value, draft.Version, draft.Title, draft.Note,
                draft.CounterpartyId, customer, draft.CreatedAtUtc, draft.UpdatedAtUtc,
                lines, null, "Unavailable", error.Code);
        }
    }
}
