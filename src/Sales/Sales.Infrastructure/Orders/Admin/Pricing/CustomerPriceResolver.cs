using Catalog.Application.Contracts.Persistence;
using Catalog.Domain.ValueObjects;
using Sales.Application.Contracts.Orders;

namespace Sales.Infrastructure.Orders;

/// <summary>
/// Picks a customer's UAH price for products: the price type of the nearest category rule on the
/// product's category or any of its ancestors, otherwise the customer's default price type.
/// </summary>
internal sealed class CustomerPriceResolver(SalesDbContext salesDb, IReadCatalogDbContext catalogDb)
{
    internal sealed record PricedProduct(int ProductId, string Name, decimal Stock, decimal? UnitPrice);

    internal async Task<IReadOnlyDictionary<int, PricedProduct>> ResolveAsync(
        string counterpartyId, IReadOnlyCollection<int> ids, CancellationToken ct)
    {
        var customer = await salesDb.Counterparties.AsNoTracking()
            .Where(x => x.Id == counterpartyId && !x.IsDeleted)
            .Select(x => new { x.Id, x.DefaultPriceTypeId })
            .SingleOrDefaultAsync(ct);
        if (customer is null)
            throw new WorkspaceException(404, "customer_not_found", "Customer does not exist.");

        var rules = (await salesDb.CounterpartyCategoryPriceTypes.AsNoTracking()
                .Where(x => x.CounterpartyId == counterpartyId)
                .Select(x => new { x.CategoryId, x.PriceTypeId })
                .ToListAsync(ct))
            .GroupBy(x => x.CategoryId)
            .ToDictionary(x => x.Key, x => x.First().PriceTypeId);
        var productIds = ids.Select(ProductId.From).ToArray();
        var products = await catalogDb.Products.AsNoTracking()
            .Where(x => productIds.Contains(x.Id))
            .Select(x => new { x.Id, x.Name, x.CategoryId, x.Stock })
            .ToListAsync(ct);
        var parents = rules.Count == 0
            ? new Dictionary<int, int?>()
            : (await catalogDb.Categories.AsNoTracking()
                    .Select(x => new { x.Id, x.ParentId })
                    .ToListAsync(ct))
                .ToDictionary(x => x.Id.Value, x => x.ParentId?.Value);
        var prices = (await catalogDb.ProductPrices.AsNoTracking()
                .Where(x => productIds.Contains(x.ProductId))
                .Select(x => new { x.ProductId, x.PriceTypeId, x.Amount, x.CurrencyIso })
                .ToListAsync(ct))
            .Where(x => x.CurrencyIso == "UAH")
            .ToLookup(x => (ProductId: x.ProductId.Value, PriceTypeId: x.PriceTypeId.Value), x => x.Amount);

        return products.ToDictionary(x => x.Id.Value, product =>
        {
            var typeId = RulePriceType(product.CategoryId.Value, rules, parents) ?? customer.DefaultPriceTypeId;
            var price = prices[(product.Id.Value, typeId)].Cast<decimal?>().FirstOrDefault();
            return new PricedProduct(product.Id.Value, product.Name, product.Stock, price);
        });
    }

    internal async Task<WorkspaceCustomerPrices> GetCustomerPricesAsync(
        WorkspaceCustomerPricesRequest request, CancellationToken ct)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.CounterpartyId) ||
            request.CounterpartyId.Trim().Length > 64 ||
            request.ProductIds is null || request.ProductIds.Count is < 1 or > 100 ||
            request.ProductIds.Any(x => !AdminWorkspacePricing.TryParseProductId(x, out _)))
            throw new WorkspaceException(400, "invalid_products", "Supply a customer and 1-100 product IDs.");

        var counterpartyId = request.CounterpartyId.Trim();
        var ids = request.ProductIds.Distinct(StringComparer.Ordinal)
            .Select(x => int.Parse(x, System.Globalization.CultureInfo.InvariantCulture)).ToArray();
        var priced = await ResolveAsync(counterpartyId, ids, ct);
        return new WorkspaceCustomerPrices(counterpartyId, ids
            .Select(id => new WorkspaceCustomerPrice(id.ToString(System.Globalization.CultureInfo.InvariantCulture),
                priced.TryGetValue(id, out var product) ? product.UnitPrice : null))
            .ToArray());
    }

    private static int? RulePriceType(int categoryId, IReadOnlyDictionary<int, int> rules,
        IReadOnlyDictionary<int, int?> parents)
    {
        int? current = categoryId;
        // The depth guard stops a malformed parent cycle from looping forever.
        for (var depth = 0; current is { } id && depth < 32; depth++)
        {
            if (rules.TryGetValue(id, out var typeId))
                return typeId;
            current = parents.TryGetValue(id, out var parent) ? parent : null;
        }
        return null;
    }
}
