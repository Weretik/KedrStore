using Catalog.Application.Contracts.Integrations;
using Catalog.Domain.ValueObjects;

namespace Catalog.Application.Contracts.Persistence;

public interface IProductPhotoCheckStore
{
    Task<IReadOnlyList<ProductPhotoCandidate>> GetActiveProductBatchAsync(
        int afterProductId,
        int take,
        CancellationToken cancellationToken);

    Task ReconcileAsync(
        IReadOnlyCollection<ProductPhotoCandidate> products,
        CancellationToken cancellationToken);

    Task<int> SaveResultsAsync(
        IReadOnlyCollection<ProductPhotoCheckUpdate> updates,
        DateTimeOffset checkedAtUtc,
        CancellationToken cancellationToken);
}

public sealed record ProductPhotoCandidate(ProductId ProductId, string PhotoUrl);

public sealed record ProductPhotoCheckUpdate(
    ProductId ProductId,
    string PhotoUrl,
    ProductPhotoProbeResult Result);
