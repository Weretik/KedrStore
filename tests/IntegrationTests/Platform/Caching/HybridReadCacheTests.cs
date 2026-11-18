using BuildingBlocks.Application.Caching;
using BuildingBlocks.Infrastructure.Caching;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.Platform.Caching;

/// <summary>The HybridCache adapter behind <c>IReadCache</c> (ADR-0016), as the host registers it.</summary>
public sealed class HybridReadCacheTests
{
    private static readonly ReadCacheEntry Entry = ReadCacheEntry.Create("test.read", "test:tag", TimeSpan.FromMinutes(1));

    [Fact]
    public async Task SecondRead_ComesFromMemory()
    {
        var cache = Create();
        var reads = 0;

        var first = await cache.GetOrCreateAsync(Entry, _ => ValueTask.FromResult(new Answer(++reads)), CancellationToken.None);
        var second = await cache.GetOrCreateAsync(Entry, _ => ValueTask.FromResult(new Answer(++reads)), CancellationToken.None);

        Assert.Equal(1, reads);
        Assert.Same(first, second);
    }

    [Fact]
    public async Task InvalidatedTag_ReadsAgain()
    {
        var cache = Create();
        var reads = 0;
        await cache.GetOrCreateAsync(Entry, _ => ValueTask.FromResult(new Answer(++reads)), CancellationToken.None);

        await cache.InvalidateAsync("test:tag", CancellationToken.None);
        var after = await cache.GetOrCreateAsync(Entry, _ => ValueTask.FromResult(new Answer(++reads)), CancellationToken.None);

        Assert.Equal(2, reads);
        Assert.Equal(2, after.Value);
    }

    [Fact]
    public async Task OtherTag_KeepsTheAnswer()
    {
        var cache = Create();
        var reads = 0;
        await cache.GetOrCreateAsync(Entry, _ => ValueTask.FromResult(new Answer(++reads)), CancellationToken.None);

        await cache.InvalidateAsync("other:tag", CancellationToken.None);
        await cache.GetOrCreateAsync(Entry, _ => ValueTask.FromResult(new Answer(++reads)), CancellationToken.None);

        Assert.Equal(1, reads);
    }

    [Fact]
    public async Task Variants_AreCachedApart()
    {
        var cache = Create();
        var uk = ReadCacheEntry.Create("test.read", "test:tag", TimeSpan.FromMinutes(1), "uk");
        var ru = ReadCacheEntry.Create("test.read", "test:tag", TimeSpan.FromMinutes(1), "ru");

        var first = await cache.GetOrCreateAsync(uk, _ => ValueTask.FromResult(new Answer(1)), CancellationToken.None);
        var second = await cache.GetOrCreateAsync(ru, _ => ValueTask.FromResult(new Answer(2)), CancellationToken.None);

        Assert.Equal(1, first.Value);
        Assert.Equal(2, second.Value);
    }

    internal static IReadCache Create()
        => new ServiceCollection().AddLogging().AddReadCache().BuildServiceProvider().GetRequiredService<IReadCache>();

    [System.ComponentModel.ImmutableObject(true)]
    public sealed record Answer(int Value);
}
