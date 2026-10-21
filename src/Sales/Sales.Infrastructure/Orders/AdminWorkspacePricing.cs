using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Catalog.Application.Contracts.Persistence;
using Catalog.Domain.ValueObjects;
using Sales.Application.Contracts.Orders;

namespace Sales.Infrastructure.Orders;

internal sealed class AdminWorkspacePricing(SalesDbContext salesDb, IReadCatalogDbContext catalogDb)
{
    internal static void ValidateRequest(WorkspaceQuoteRequest? request)
    {
        if (request is null ||
            string.IsNullOrWhiteSpace(request.CounterpartyId) ||
            request.CounterpartyId.Trim().Length > 64 ||
            request.Lines is null || request.Lines.Count is < 1 or > 100 ||
            request.Lines.Any(x => x is null || x.Quantity < 1 ||
                !int.TryParse(x.ProductId, NumberStyles.None, CultureInfo.InvariantCulture, out var id) ||
                id is < 1 or > 1_000_000_000 ||
                x.ProductId != id.ToString(CultureInfo.InvariantCulture)) ||
            request.Lines.Select(x => x.ProductId).Distinct(StringComparer.Ordinal).Count() != request.Lines.Count)
            throw new WorkspaceException(400, "invalid_lines", "Supply a customer and 1-100 unique products with positive quantities.");
    }
    internal async Task<IReadOnlyList<WorkspaceDraftQuotedLine>> DescribeDraftLinesAsync(
        IReadOnlyCollection<OrderDraftLine> lines, CancellationToken ct)
    {
        var ids = lines.Select(x => int.TryParse(x.ProductId, out var id) ? id : -1)
            .Where(x => x is > 0 and <= 1_000_000_000).Distinct().Select(ProductId.From).ToArray();
        var products = await catalogDb.Products.AsNoTracking()
            .Where(x => ids.Contains(x.Id))
            .Select(x => new { x.Id, x.Name })
            .ToDictionaryAsync(x => x.Id.Value, x => x.Name, ct);
        return lines.Select(x =>
        {
            string? name = null;
            if (int.TryParse(x.ProductId, out var id))
                products.TryGetValue(id, out name);
            return new WorkspaceDraftQuotedLine(x.ProductId, name, x.Quantity, null, null);
        }).ToArray();
    }

    public async Task<WorkspaceQuote> QuoteAsync(WorkspaceQuoteRequest request, CancellationToken ct)
    {
        ValidateRequest(request);

        var counterpartyId = request.CounterpartyId.Trim();
        var customer = await salesDb.Counterparties.AsNoTracking()
            .Where(x => x.Id == counterpartyId && !x.IsDeleted)
            .Select(x => new { x.Id, x.DefaultPriceTypeId })
            .SingleOrDefaultAsync(ct);
        if (customer is null)
            throw new WorkspaceException(404, "customer_not_found", "Customer does not exist.");

        var rules = await salesDb.CounterpartyCategoryPriceTypes.AsNoTracking()
            .Where(x => x.CounterpartyId == counterpartyId)
            .Select(x => new { x.CategoryId, x.PriceTypeId })
            .ToListAsync(ct);
        var ids = request.Lines.Select(x => int.Parse(x.ProductId, CultureInfo.InvariantCulture)).ToArray();
        var productIds = ids.Select(ProductId.From).ToArray();
        var products = await catalogDb.Products.AsNoTracking()
            .Where(x => productIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Name, x.CategoryId, x.Stock })
            .ToListAsync(ct);
        if (products.Count != ids.Length)
            throw new WorkspaceException(404, "product_not_found", "Product does not exist.");

        var prices = await catalogDb.ProductPrices.AsNoTracking()
            .Where(x => productIds.Contains(x.ProductId))
            .Select(x => new { x.ProductId, x.PriceTypeId, x.Amount, x.CurrencyIso })
            .ToListAsync(ct);

        var quoteLines = new List<WorkspaceQuoteLine>(request.Lines.Count);
        foreach (var line in request.Lines)
        {
            var productId = int.Parse(line.ProductId, CultureInfo.InvariantCulture);
            var product = products.Single(x => x.Id.Value == productId);
            if (product.Stock < line.Quantity)
                throw new WorkspaceException(400, "unavailable_product", "Product stock is insufficient.");
            var typeId = rules.FirstOrDefault(x => x.CategoryId == product.CategoryId.Value)?.PriceTypeId
                ?? customer.DefaultPriceTypeId;
            var price = prices.FirstOrDefault(x => x.ProductId.Value == productId && x.PriceTypeId.Value == typeId);
            if (price is null || price.CurrencyIso != "UAH")
                throw new WorkspaceException(400, "unavailable_price", "A UAH price is unavailable for a product.");
            decimal lineAmount;
            try { lineAmount = checked(price.Amount * line.Quantity); }
            catch (OverflowException) { throw new WorkspaceException(400, "amount_overflow", "The order amount is too large."); }
            quoteLines.Add(new WorkspaceQuoteLine(line.ProductId, product.Name, line.Quantity, price.Amount, lineAmount));
        }

        decimal total;
        try { total = quoteLines.Aggregate(0m, (sum, line) => checked(sum + line.LineAmount)); }
        catch (OverflowException) { throw new WorkspaceException(400, "amount_overflow", "The order amount is too large."); }
        var canonical = string.Join("|", quoteLines.OrderBy(x => x.ProductId, StringComparer.Ordinal)
            .Select(x => $"{x.ProductId}:{x.Quantity}:{x.UnitPrice.ToString(CultureInfo.InvariantCulture)}"));
        var fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{counterpartyId}|{canonical}")));
        return new WorkspaceQuote(counterpartyId, quoteLines, total, fingerprint);
    }
}
