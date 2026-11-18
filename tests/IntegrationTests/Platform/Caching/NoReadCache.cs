using BuildingBlocks.Application.Caching;

namespace IntegrationTests.Platform.Caching;

/// <summary>
/// An <see cref="IReadCache"/> that keeps nothing: every read runs. Tests that call handlers directly, without the
/// invalidation behavior of the pipeline, use it so a query after a command sees the change.
/// </summary>
internal sealed class NoReadCache : IReadCache
{
    public static readonly NoReadCache Instance = new();

    public ValueTask<T> GetOrCreateAsync<T>(ReadCacheEntry entry, Func<CancellationToken, ValueTask<T>> read,
        CancellationToken cancellationToken)
        => read(cancellationToken);

    public ValueTask InvalidateAsync(string tag, CancellationToken cancellationToken) => ValueTask.CompletedTask;
}
