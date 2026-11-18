using System.Diagnostics;
using BuildingBlocks.Application.Caching;
using Microsoft.Extensions.Caching.Hybrid;

namespace BuildingBlocks.Infrastructure.Caching;

/// <summary>
/// <see cref="IReadCache"/> over .NET <see cref="HybridCache"/> (ADR-0016): stampede protection, tags, and an optional
/// L2 picked up from a registered <c>IDistributedCache</c>. Each read tags the request span in Cloud Trace with
/// <c>cache.&lt;name&gt; = hit | miss</c>.
/// </summary>
public sealed class HybridReadCache(HybridCache cache) : IReadCache
{
    public async ValueTask<T> GetOrCreateAsync<T>(
        ReadCacheEntry entry,
        Func<CancellationToken, ValueTask<T>> read,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(read);
        var state = new ReadState<T>(read);
        var options = new HybridCacheEntryOptions { Expiration = entry.Lifetime, LocalCacheExpiration = entry.Lifetime };
        var value = await cache.GetOrCreateAsync(
            entry.Key, state, static (current, token) => current.ReadAsync(token), options, [entry.Tag],
            cancellationToken);
        Activity.Current?.SetTag($"cache.{entry.Name}", state.Missed ? "miss" : "hit");
        return value;
    }

    public ValueTask InvalidateAsync(string tag, CancellationToken cancellationToken)
        => cache.RemoveByTagAsync(tag, cancellationToken);

    private sealed class ReadState<T>(Func<CancellationToken, ValueTask<T>> read)
    {
        public bool Missed { get; private set; }

        public ValueTask<T> ReadAsync(CancellationToken cancellationToken)
        {
            Missed = true;
            return read(cancellationToken);
        }
    }
}
