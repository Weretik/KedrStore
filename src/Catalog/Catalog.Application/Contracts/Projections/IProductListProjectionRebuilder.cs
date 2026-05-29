namespace Catalog.Application.Contracts.Projections;

public interface IProductListProjectionRebuilder
{
    Task RebuildAsync(CancellationToken cancellationToken = default);
}
