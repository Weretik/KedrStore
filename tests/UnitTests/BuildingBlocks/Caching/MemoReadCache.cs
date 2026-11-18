using BuildingBlocks.Application.Caching;

namespace UnitTests.BuildingBlocks.Caching;

/// <summary>
/// A plain in-test <see cref="IReadCache"/>: keeps answers by key and drops them by tag, with no expiry. The HybridCache
/// adapter itself is covered in IntegrationTests (Platform/Caching).
/// </summary>
internal sealed class MemoReadCache : IReadCache
{
    private readonly Dictionary<string, (string Tag, object? Value)> answers = new(StringComparer.Ordinal);

    public async ValueTask<T> GetOrCreateAsync<T>(
        ReadCacheEntry entry, Func<CancellationToken, ValueTask<T>> read, CancellationToken cancellationToken)
    {
        if (answers.TryGetValue(entry.Key, out var cached)) return (T)cached.Value!;
        var value = await read(cancellationToken);
        answers[entry.Key] = (entry.Tag, value);
        return value;
    }

    public ValueTask InvalidateAsync(string tag, CancellationToken cancellationToken)
    {
        foreach (var key in answers.Where(x => x.Value.Tag == tag).Select(x => x.Key).ToList()) answers.Remove(key);
        return ValueTask.CompletedTask;
    }
}
