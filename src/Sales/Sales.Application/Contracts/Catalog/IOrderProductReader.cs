namespace Sales.Application.Contracts.Catalog;

public interface IOrderProductReader
{
    Task<IReadOnlyDictionary<string, OrderProductSnapshot>> GetByIdsAsync(
        IReadOnlyCollection<string> productIds,
        CancellationToken cancellationToken);
}
