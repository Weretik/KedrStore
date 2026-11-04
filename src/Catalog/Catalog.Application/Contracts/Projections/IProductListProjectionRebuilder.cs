namespace Catalog.Application.Contracts.Projections;

public interface IProductListProjectionRebuilder
{
    Task RebuildAsync(CancellationToken cancellationToken = default);

    /// <summary>Copies the cached photo of every product into its list row where it differs (one statement).</summary>
    Task<int> RefreshPhotosAsync(CancellationToken cancellationToken = default);
}
