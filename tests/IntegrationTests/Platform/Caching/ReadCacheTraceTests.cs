using System.Diagnostics;
using BuildingBlocks.Application.Caching;

namespace IntegrationTests.Platform.Caching;

/// <summary>Each cached read tags the current request span, so Cloud Trace shows hits and misses.</summary>
public sealed class ReadCacheTraceTests
{
    private static readonly ReadCacheEntry Entry = ReadCacheEntry.Create("test.read", "test:tag", TimeSpan.FromMinutes(1));

    [Fact]
    public async Task Read_TagsTheRequestSpanWithHitOrMiss()
    {
        var cache = HybridReadCacheTests.Create();
        using var listener = new ActivityListener
        {
            ShouldListenTo = _ => true,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
        };
        ActivitySource.AddActivityListener(listener);
        using var source = new ActivitySource("UnitTests.Caching");

        using (var miss = source.StartActivity("miss"))
        {
            await cache.GetOrCreateAsync(Entry, _ => ValueTask.FromResult(new HybridReadCacheTests.Answer(1)), CancellationToken.None);
            Assert.Equal("miss", miss!.GetTagItem("cache.test.read"));
        }

        using var hit = source.StartActivity("hit");
        await cache.GetOrCreateAsync(Entry, _ => ValueTask.FromResult(new HybridReadCacheTests.Answer(2)), CancellationToken.None);
        Assert.Equal("hit", hit!.GetTagItem("cache.test.read"));
    }
}
