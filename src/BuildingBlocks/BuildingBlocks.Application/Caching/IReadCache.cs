namespace BuildingBlocks.Application.Caching;

/// <summary>
/// Port for caching the result of a read (ADR-0016). Queries ask it for a ready answer; commands drop answers by tag.
/// The adapter is <c>HybridReadCache</c> over .NET <c>HybridCache</c>: memory of this instance only (L1), with a shared
/// L2 (for example Redis) added later by registering an <c>IDistributedCache</c>, without changing any caller.
/// </summary>
public interface IReadCache
{
    /// <summary>
    /// The cached answer for <paramref name="entry"/>, or the result of <paramref name="read"/>, which is then kept for
    /// <see cref="ReadCacheEntry.Lifetime"/>. Concurrent callers of a missing key wait for one <paramref name="read"/>.
    /// </summary>
    ValueTask<T> GetOrCreateAsync<T>(
        ReadCacheEntry entry,
        Func<CancellationToken, ValueTask<T>> read,
        CancellationToken cancellationToken);

    /// <summary>Drops every answer that carries <paramref name="tag"/> on this instance (and in L2 when it exists).</summary>
    ValueTask InvalidateAsync(string tag, CancellationToken cancellationToken);
}
