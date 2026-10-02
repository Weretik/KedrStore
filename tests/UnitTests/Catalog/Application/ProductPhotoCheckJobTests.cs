using Catalog.Application.Contracts.Integrations;
using Catalog.Application.Contracts.Persistence;
using Catalog.Application.Jobs.ProductPhotos;
using Catalog.Domain.Enums;
using Catalog.Domain.ValueObjects;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace UnitTests.Catalog.Application;

public sealed class ProductPhotoCheckJobTests
{
    [Fact]
    public async Task RunAsync_ProcessesDeterministicBatchesAndCountsStatuses()
    {
        var products = Enumerable.Range(1, 5)
            .Select(id => Candidate(id))
            .ToArray();
        var store = new RecordingStore(products) { StaleResults = 1 };
        var probe = new StubProbe(candidateUrl => candidateUrl.EndsWith("/1.jpg", StringComparison.Ordinal)
            ? Result(ProductPhotoStatus.Available, 200)
            : candidateUrl.EndsWith("/2.jpg", StringComparison.Ordinal)
                ? Result(ProductPhotoStatus.Missing, 404, "http_missing")
                : candidateUrl.EndsWith("/3.jpg", StringComparison.Ordinal)
                    ? Result(ProductPhotoStatus.InvalidContentType, 200, "invalid_content_type")
                    : Result(ProductPhotoStatus.CheckFailed, 500, "http_500"));

        var summary = await CreateJob(store, probe).RunAsync(CancellationToken.None);

        Assert.Equal(5, summary.Examined);
        Assert.Equal(1, summary.Available);
        Assert.Equal(1, summary.Missing);
        Assert.Equal(1, summary.InvalidContentType);
        Assert.Equal(2, summary.CheckFailed);
        Assert.Equal(3, summary.StaleResults);
        Assert.Equal([0, 2, 4], store.RequestedAfterIds);
        Assert.Equal([1, 2, 3, 4, 5], store.SavedUpdates.Select(update => update.ProductId.Value));
    }

    [Fact]
    public async Task RunAsync_UnexpectedProbeFailureBecomesCheckFailedAndContinues()
    {
        var store = new RecordingStore([Candidate(1), Candidate(2)]);
        var probe = new StubProbe(url =>
        {
            if (url.EndsWith("/1.jpg", StringComparison.Ordinal))
                throw new InvalidOperationException("unexpected");
            return Result(ProductPhotoStatus.Available, 200);
        });

        var summary = await CreateJob(store, probe).RunAsync(CancellationToken.None);

        Assert.Equal(2, summary.Examined);
        Assert.Equal(1, summary.CheckFailed);
        Assert.Equal(1, summary.Available);
        Assert.Contains(store.SavedUpdates, update =>
            update.ProductId.Value == 1 && update.Result.DiagnosticCode == "unexpected_probe_error");
    }

    [Fact]
    public async Task RunAsync_PropagatesCallerCancellation()
    {
        var store = new RecordingStore([Candidate(1)]);
        using var source = new CancellationTokenSource();
        source.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            CreateJob(store, new StubProbe(_ => Result(ProductPhotoStatus.Available, 200)))
                .RunAsync(source.Token));

        Assert.Empty(store.SavedUpdates);
    }

    private static CheckProductPhotosJob CreateJob(
        IProductPhotoCheckStore store,
        IProductPhotoProbe probe)
        => new(
            store,
            probe,
            Options.Create(new ProductPhotoCheckOptions { BatchSize = 2, MaxConcurrency = 2 }),
            TimeProvider.System,
            NullLogger<CheckProductPhotosJob>.Instance);

    private static ProductPhotoCandidate Candidate(int id)
        => new(ProductId.From(id), $"https://images-kedr.cdn.express/products/{id}.jpg");

    private static ProductPhotoProbeResult Result(
        ProductPhotoStatus status,
        int? httpStatus,
        string? diagnosticCode = null)
        => new(status, httpStatus, diagnosticCode, null);

    private sealed class StubProbe(Func<string, ProductPhotoProbeResult> resultFactory) : IProductPhotoProbe
    {
        public Task<ProductPhotoProbeResult> ProbeAsync(string photoUrl, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(resultFactory(photoUrl));
        }
    }

    private sealed class RecordingStore(IReadOnlyList<ProductPhotoCandidate> products) : IProductPhotoCheckStore
    {
        public List<int> RequestedAfterIds { get; } = [];
        public List<ProductPhotoCheckUpdate> SavedUpdates { get; } = [];
        public int StaleResults { get; init; }

        public Task<IReadOnlyList<ProductPhotoCandidate>> GetActiveProductBatchAsync(
            int afterProductId,
            int take,
            CancellationToken cancellationToken)
        {
            RequestedAfterIds.Add(afterProductId);
            IReadOnlyList<ProductPhotoCandidate> batch = products
                .Where(product => product.ProductId.Value > afterProductId)
                .OrderBy(product => product.ProductId.Value)
                .Take(take)
                .ToArray();
            return Task.FromResult(batch);
        }

        public Task ReconcileAsync(
            IReadOnlyCollection<ProductPhotoCandidate> candidates,
            CancellationToken cancellationToken)
            => Task.CompletedTask;

        public Task<int> SaveResultsAsync(
            IReadOnlyCollection<ProductPhotoCheckUpdate> updates,
            DateTimeOffset checkedAtUtc,
            CancellationToken cancellationToken)
        {
            SavedUpdates.AddRange(updates);
            return Task.FromResult(StaleResults);
        }
    }
}
