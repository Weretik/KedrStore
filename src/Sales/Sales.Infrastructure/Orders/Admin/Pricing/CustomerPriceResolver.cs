using Catalog.Contracts.Sales;
using Sales.Application.Contracts.Orders;
using Sales.Application.Features.Orders.AdminWorkspace;

namespace Sales.Infrastructure.Orders;

/// <summary>
/// Picks a customer's UAH price for products: the price type of the nearest category rule on the
/// product's category or any of its ancestors, otherwise the customer's default price type.
/// </summary>
internal sealed class CustomerPriceResolver(SalesDbContext salesDb, ICatalogSalesReader catalog)
{
    internal sealed record PricedProduct(int ProductId, string Name, decimal Stock, decimal? UnitPrice);

    internal async Task<DomainResult<IReadOnlyDictionary<int, PricedProduct>>> ResolveAsync(
        string counterpartyId, IReadOnlyCollection<int> ids, CancellationToken ct)
    {
        var customer = await salesDb.Counterparties.AsNoTracking()
            .Where(x => x.Id == counterpartyId && !x.IsDeleted)
            .Select(x => new { x.Id, x.DefaultPriceTypeId })
            .SingleOrDefaultAsync(ct);
        if (customer is null)
            return WorkspaceErrors.CustomerNotFound();

        var rules = (await salesDb.CounterpartyCategoryPriceTypes.AsNoTracking()
                .Where(x => x.CounterpartyId == counterpartyId)
                .Select(x => new { x.CategoryId, x.PriceTypeId })
                .ToListAsync(ct))
            .GroupBy(x => x.CategoryId)
            .ToDictionary(x => x.Key, x => x.First().PriceTypeId);
        var products = await catalog.GetProductsAsync(ids, ct);
        var parents = rules.Count == 0
            ? new Dictionary<int, int?>()
            : await catalog.GetCategoryParentsAsync(ct);
        var prices = (await catalog.GetPricesAsync(ids, "UAH", ct))
            .ToLookup(x => (x.ProductId, x.PriceTypeId), x => x.Amount);

        return DomainResult<IReadOnlyDictionary<int, PricedProduct>>.Success(products.ToDictionary(x => x.Id, product =>
        {
            var typeId = RulePriceType(product.CategoryId, rules, parents) ?? customer.DefaultPriceTypeId;
            var price = prices[(product.Id, typeId)].Cast<decimal?>().FirstOrDefault();
            return new PricedProduct(product.Id, product.Name, product.Stock, price);
        }));
    }

    /// <summary>The request passed the workspace validator.</summary>
    internal async Task<DomainResult<WorkspaceCustomerPrices>> GetCustomerPricesAsync(
        WorkspaceCustomerPricesRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        var counterpartyId = request.CounterpartyId.Trim();
        var ids = request.ProductIds.Distinct(StringComparer.Ordinal)
            .Select(x => int.Parse(x, System.Globalization.CultureInfo.InvariantCulture)).ToArray();
        var priced = await ResolveAsync(counterpartyId, ids, ct);
        if (priced.IsFailure)
            return priced.Error!;
        return new WorkspaceCustomerPrices(counterpartyId, ids
            .Select(id => new WorkspaceCustomerPrice(id.ToString(System.Globalization.CultureInfo.InvariantCulture),
                priced.Value.TryGetValue(id, out var product) ? product.UnitPrice : null))
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
