using System.Collections.Concurrent;
using Catalog.Application.Contracts.Integrations;
using Catalog.Application.Contracts.Persistence;
using Catalog.Domain.Enums;

namespace Catalog.Application.Jobs.ProductPhotos;

public sealed class CheckProductPhotosJob(
    IProductPhotoCheckStore store,
    IProductPhotoProbe probe,
    IOptions<ProductPhotoCheckOptions> options,
    TimeProvider timeProvider,
    ILogger<CheckProductPhotosJob> logger)
{
    private readonly ProductPhotoCheckOptions _options = Validate(options.Value);

    public async Task<ProductPhotoCheckSummary> RunAsync(CancellationToken cancellationToken)
    {
        var startedAt = timeProvider.GetUtcNow();
        var afterProductId = 0;
        var examined = 0;
        var available = 0;
        var missing = 0;
        var invalidContentType = 0;
        var checkFailed = 0;
        var staleResults = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var batch = await store.GetActiveProductBatchAsync(
                afterProductId,
                _options.BatchSize,
                cancellationToken);
            if (batch.Count == 0)
                break;

            var updates = new ConcurrentBag<ProductPhotoCheckUpdate>();
            await Parallel.ForEachAsync(
                batch,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = _options.MaxConcurrency,
                    CancellationToken = cancellationToken
                },
                async (candidate, token) =>
                {
                    ProductPhotoProbeResult result;
                    try
                    {
                        result = await probe.ProbeAsync(candidate.PhotoUrl, token);
                    }
                    catch (OperationCanceledException) when (token.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception exception)
                    {
                        logger.LogWarning(
                            exception,
                            "Unexpected product photo probe failure. ProductId={ProductId}.",
                            candidate.ProductId.Value);
                        result = new(
                            ProductPhotoStatus.CheckFailed,
                            null,
                            "unexpected_probe_error",
                            "The photo probe failed unexpectedly.");
                    }

                    updates.Add(new(candidate.ProductId, candidate.PhotoUrl, result));
                });

            var orderedUpdates = updates.OrderBy(update => update.ProductId.Value).ToArray();
            staleResults += await store.SaveResultsAsync(
                orderedUpdates,
                timeProvider.GetUtcNow(),
                cancellationToken);

            examined += orderedUpdates.Length;
            foreach (var update in orderedUpdates)
            {
                switch (update.Result.Status)
                {
                    case ProductPhotoStatus.Available:
                        available++;
                        break;
                    case ProductPhotoStatus.Missing:
                        missing++;
                        break;
                    case ProductPhotoStatus.InvalidContentType:
                        invalidContentType++;
                        break;
                    case ProductPhotoStatus.CheckFailed:
                        checkFailed++;
                        break;
                    case ProductPhotoStatus.Unknown:
                    default:
                        throw new InvalidOperationException("A photo probe must return a terminal status.");
                }
            }

            afterProductId = batch[^1].ProductId.Value;
            if (batch.Count < _options.BatchSize)
                break;
        }

        var summary = new ProductPhotoCheckSummary(
            examined,
            available,
            missing,
            invalidContentType,
            checkFailed,
            staleResults,
            timeProvider.GetUtcNow() - startedAt);

        logger.LogInformation(
            "Product photo check completed. Examined={Examined}, Available={Available}, Missing={Missing}, InvalidContentType={InvalidContentType}, CheckFailed={CheckFailed}, StaleResults={StaleResults}, ElapsedMs={ElapsedMs}.",
            summary.Examined,
            summary.Available,
            summary.Missing,
            summary.InvalidContentType,
            summary.CheckFailed,
            summary.StaleResults,
            summary.Elapsed.TotalMilliseconds);

        return summary;
    }

    private static ProductPhotoCheckOptions Validate(ProductPhotoCheckOptions options)
    {
        if (options.BatchSize is < 1 or > 5000)
            throw new InvalidOperationException("ProductPhotoCheck:BatchSize must be between 1 and 5000.");
        if (options.MaxConcurrency is < 1 or > 100)
            throw new InvalidOperationException("ProductPhotoCheck:MaxConcurrency must be between 1 and 100.");

        return options;
    }
}

public sealed record ProductPhotoCheckSummary(
    int Examined,
    int Available,
    int Missing,
    int InvalidContentType,
    int CheckFailed,
    int StaleResults,
    TimeSpan Elapsed);
