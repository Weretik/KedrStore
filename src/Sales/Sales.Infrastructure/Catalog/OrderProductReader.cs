using Catalog.Contracts.Sales;
using Sales.Application.Contracts.Catalog;
using System.Globalization;

namespace Sales.Infrastructure.Catalog;

internal sealed class OrderProductReader(ICatalogSalesReader catalog) : IOrderProductReader
{
    public async Task<IReadOnlyDictionary<string, OrderProductSnapshot>> GetByIdsAsync(
        IReadOnlyCollection<string> productIds,
        CancellationToken cancellationToken)
    {
        var parsedIds = productIds
            .Select(id => int.TryParse(id, out var value) && value > 0 ? (Original: id, Value: value) : ((string Original, int Value)?)null)
            .ToArray();
        if (parsedIds.Any(id => id is null))
            return new Dictionary<string, OrderProductSnapshot>();

        var products = await catalog.GetProductsAsync(
            parsedIds.Select(id => id!.Value.Value).ToArray(), cancellationToken);

        return products.ToDictionary(
            product => product.Id.ToString(CultureInfo.InvariantCulture),
            product => new OrderProductSnapshot(product.Id.ToString(CultureInfo.InvariantCulture), product.Name),
            StringComparer.Ordinal);
    }
}
