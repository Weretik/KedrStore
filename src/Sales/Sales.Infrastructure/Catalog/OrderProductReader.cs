using Catalog.Application.Contracts.Persistence;
using Catalog.Domain.ValueObjects;
using Sales.Application.Contracts.Catalog;
using System.Globalization;

namespace Sales.Infrastructure.Catalog;

internal sealed class OrderProductReader(IReadCatalogDbContext catalogDbContext) : IOrderProductReader
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

        var ids = parsedIds.Select(id => ProductId.From(id!.Value.Value)).ToArray();
        var products = await catalogDbContext.Products
            .AsNoTracking()
            .Where(product => ids.Contains(product.Id))
            .Select(product => new { Id = product.Id.Value, product.Name })
            .ToListAsync(cancellationToken);

        return products.ToDictionary(
            product => product.Id.ToString(CultureInfo.InvariantCulture),
            product => new OrderProductSnapshot(product.Id.ToString(CultureInfo.InvariantCulture), product.Name),
            StringComparer.Ordinal);
    }
}
