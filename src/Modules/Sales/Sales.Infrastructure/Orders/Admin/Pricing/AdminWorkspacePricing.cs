using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Catalog.Contracts.Sales;
using Sales.Application.Abstractions.Orders;
using Sales.Application.Features.OrderWorkspace.Shared;
using Sales.Domain.Orders.Pricing;

namespace Sales.Infrastructure.Orders;

internal sealed class AdminWorkspacePricing(ICatalogSalesReader catalog, CustomerPriceResolver prices) : IWorkspacePricing
{
    internal async Task<IReadOnlyList<WorkspaceDraftQuotedLine>> DescribeDraftLinesAsync(
        IReadOnlyCollection<OrderDraftLine> lines, CancellationToken ct)
    {
        var ids = lines.Select(x => int.TryParse(x.ProductId, out var id) ? id : -1)
            .Where(x => x is > 0 and <= 1_000_000_000).Distinct().ToArray();
        var products = (await catalog.GetProductsAsync(ids, ct)).ToDictionary(x => x.Id, x => x.Name);
        return lines.Select(x =>
        {
            string? name = null;
            if (int.TryParse(x.ProductId, out var id))
                products.TryGetValue(id, out name);
            return new WorkspaceDraftQuotedLine(x.ProductId, name, x.Quantity, null, null, x.PriceAdjustment, null);
        }).ToArray();
    }

    public Task<DomainResult<WorkspaceCustomerPrices>> GetCustomerPricesAsync(
        WorkspaceCustomerPricesRequest request, CancellationToken ct)
        => prices.GetCustomerPricesAsync(request, ct);

    /// <summary>
    /// Prices the lines for the customer. The request passed the workspace validators; a stored draft is checked here
    /// again because its lines were saved under the rules of that time.
    /// </summary>
    public async Task<DomainResult<WorkspaceQuote>> QuoteAsync(WorkspaceQuoteRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var lines = WorkspaceInputRules.CheckLines(request.CounterpartyId, request.Lines);
        if (lines.IsFailure)
            return lines.Error!;

        var counterpartyId = request.CounterpartyId.Trim();
        var ids = request.Lines.Select(x => int.Parse(x.ProductId, CultureInfo.InvariantCulture)).ToArray();
        var resolved = await prices.ResolveAsync(counterpartyId, ids, ct);
        if (resolved.IsFailure)
            return resolved.Error!;
        var products = resolved.Value;
        if (products.Count != ids.Length)
            return WorkspaceErrors.ProductNotFound();

        var quoteLines = new List<WorkspaceQuoteLine>(request.Lines.Count);
        foreach (var line in request.Lines)
        {
            var product = products[int.Parse(line.ProductId, CultureInfo.InvariantCulture)];
            var priced = OrderLinePricing.Price(product.Name, product.Stock, product.UnitPrice, line.Quantity,
                line.PriceAdjustment);
            if (priced.IsFailure)
                return priced.Error!;
            quoteLines.Add(new WorkspaceQuoteLine(line.ProductId, product.Name, line.Quantity, priced.Value.UnitPrice,
                priced.Value.LineAmount, priced.Value.BaseUnitPrice, line.PriceAdjustment));
        }

        var total = OrderLinePricing.Total(quoteLines.Select(line => line.LineAmount));
        if (total.IsFailure)
            return total.Error!;
        var canonical = string.Join("|", quoteLines.OrderBy(x => x.ProductId, StringComparer.Ordinal)
            .Select(x => string.Create(CultureInfo.InvariantCulture,
                $"{x.ProductId}:{x.Quantity}:{x.BaseUnitPrice:0.00##}:{x.PriceAdjustment:0.00}")));
        var fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{counterpartyId}|{canonical}")));
        return new WorkspaceQuote(counterpartyId, quoteLines, total.Value, fingerprint);
    }
}
