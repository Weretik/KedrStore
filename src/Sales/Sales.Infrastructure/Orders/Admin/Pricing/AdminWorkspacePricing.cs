using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Catalog.Application.Contracts.Persistence;
using Catalog.Domain.ValueObjects;
using Sales.Application.Contracts.Orders;

namespace Sales.Infrastructure.Orders;

internal sealed class AdminWorkspacePricing(IReadCatalogDbContext catalogDb, CustomerPriceResolver prices) : IWorkspacePricing
{
    /// <summary>Largest per-unit discount or markup, in UAH, a manager may enter.</summary>
    internal const decimal MaxPriceAdjustment = 1_000_000m;

    /// <summary>Lowest unit price, in UAH, a discount may bring a product to.</summary>
    internal const decimal MinDiscountedUnitPrice = 1m;

    internal static bool TryParseProductId(string? value, out int id)
    {
        id = 0;
        return value is not null &&
               int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out id) &&
               id is >= 1 and <= 1_000_000_000 &&
               value == id.ToString(CultureInfo.InvariantCulture);
    }

    internal static void ValidateRequest(WorkspaceQuoteRequest? request)
    {
        if (request is null ||
            string.IsNullOrWhiteSpace(request.CounterpartyId) ||
            request.CounterpartyId.Trim().Length > 64 ||
            request.Lines is null || request.Lines.Count is < 1 or > 100 ||
            request.Lines.Any(x => x is null || x.Quantity < 1 || !TryParseProductId(x.ProductId, out _)) ||
            request.Lines.Select(x => x.ProductId).Distinct(StringComparer.Ordinal).Count() != request.Lines.Count)
            throw new WorkspaceException(400, "invalid_lines", "Supply a customer and 1-100 unique products with positive quantities.");
        if (request.Lines.Any(x => Math.Abs(x.PriceAdjustment) > MaxPriceAdjustment ||
                                   decimal.Round(x.PriceAdjustment, 2) != x.PriceAdjustment))
            throw new WorkspaceException(400, "invalid_price_adjustment",
                "Знижка або націнка має бути сумою в гривнях з точністю до копійки.");
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
            return new WorkspaceDraftQuotedLine(x.ProductId, name, x.Quantity, null, null, x.PriceAdjustment, null);
        }).ToArray();
    }

    public Task<WorkspaceCustomerPrices> GetCustomerPricesAsync(WorkspaceCustomerPricesRequest request, CancellationToken ct)
        => prices.GetCustomerPricesAsync(request, ct);

    public async Task<WorkspaceQuote> QuoteAsync(WorkspaceQuoteRequest request, CancellationToken ct)
    {
        ValidateRequest(request);

        var counterpartyId = request.CounterpartyId.Trim();
        var ids = request.Lines.Select(x => int.Parse(x.ProductId, CultureInfo.InvariantCulture)).ToArray();
        var products = await prices.ResolveAsync(counterpartyId, ids, ct);
        if (products.Count != ids.Length)
            throw new WorkspaceException(404, "product_not_found", "Product does not exist.");

        var quoteLines = new List<WorkspaceQuoteLine>(request.Lines.Count);
        foreach (var line in request.Lines)
        {
            var product = products[int.Parse(line.ProductId, CultureInfo.InvariantCulture)];
            if (product.Stock < line.Quantity)
                throw new WorkspaceException(400, "unavailable_product",
                    $"Недостатньо товару «{product.Name}» на складі: доступно {product.Stock}, потрібно {line.Quantity}.");
            if (product.UnitPrice is not { } basePrice)
                throw new WorkspaceException(400, "unavailable_price", "A UAH price is unavailable for a product.");
            var unitPrice = basePrice + line.PriceAdjustment;
            // A discount may not take a unit below 1 ₴; a product already cheaper than that keeps its price.
            if (line.PriceAdjustment < 0m && unitPrice < Math.Min(MinDiscountedUnitPrice, basePrice))
                throw new WorkspaceException(400, "price_below_minimum", basePrice <= MinDiscountedUnitPrice
                    ? $"Ціна «{product.Name}» {basePrice:0.00} ₴ — знижку застосувати не можна."
                    : $"Ціна «{product.Name}» після знижки {unitPrice:0.00} ₴, а має бути щонайменше {MinDiscountedUnitPrice:0.00} ₴ за шт. Найбільша знижка — {basePrice - MinDiscountedUnitPrice:0.00} ₴.");
            decimal lineAmount;
            try { lineAmount = checked(unitPrice * line.Quantity); }
            catch (OverflowException) { throw new WorkspaceException(400, "amount_overflow", "The order amount is too large."); }
            quoteLines.Add(new WorkspaceQuoteLine(line.ProductId, product.Name, line.Quantity, unitPrice, lineAmount,
                basePrice, line.PriceAdjustment));
        }

        decimal total;
        try { total = quoteLines.Aggregate(0m, (sum, line) => checked(sum + line.LineAmount)); }
        catch (OverflowException) { throw new WorkspaceException(400, "amount_overflow", "The order amount is too large."); }
        var canonical = string.Join("|", quoteLines.OrderBy(x => x.ProductId, StringComparer.Ordinal)
            .Select(x => string.Create(CultureInfo.InvariantCulture,
                $"{x.ProductId}:{x.Quantity}:{x.BaseUnitPrice:0.00##}:{x.PriceAdjustment:0.00}")));
        var fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes($"{counterpartyId}|{canonical}")));
        return new WorkspaceQuote(counterpartyId, quoteLines, total, fingerprint);
    }
}
