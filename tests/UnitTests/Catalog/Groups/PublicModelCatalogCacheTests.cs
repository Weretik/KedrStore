using Catalog.Application.Features.SiteModels.Shared.Public;
using Catalog.Domain.Attributes;
using Catalog.Domain.Attributes.ValueObjects;
using Catalog.Domain.Categories;
using Catalog.Domain.Groups;
using Catalog.Domain.Products;

namespace UnitTests.Catalog.Groups;

/// <summary>
/// The public site reads the model catalogue once per <see cref="PublicModelCatalogCache.Lifetime"/>, not once per
/// request (audit 2026-11-15 finding E); concurrent requests wait for one load.
/// </summary>
public sealed class PublicModelCatalogCacheTests
{
    [Fact]
    public async Task LoadsOncePerLifetime_AndOnceForConcurrentRequests()
    {
        var clock = new ManualClock();
        var cache = new PublicModelCatalogCache(clock);
        var loads = 0;
        var gate = new TaskCompletionSource();
        async Task<PublicModelCatalog> Load(CancellationToken _)
        {
            Interlocked.Increment(ref loads);
            await gate.Task;
            return Catalog;
        }

        var concurrent = Enumerable.Range(0, 5).Select(_ => cache.GetAsync(Load, CancellationToken.None)).ToList();
        gate.SetResult();
        await Task.WhenAll(concurrent);
        clock.Now += PublicModelCatalogCache.Lifetime - TimeSpan.FromSeconds(1);
        await cache.GetAsync(Load, CancellationToken.None);
        Assert.Equal(1, loads);

        clock.Now += TimeSpan.FromSeconds(2);
        await cache.GetAsync(Load, CancellationToken.None);
        Assert.Equal(2, loads);
    }

    private static readonly PublicModelCatalog Catalog = new()
    {
        Products = new Dictionary<ProductId, PublicProductFacts>(),
        Models = [],
        ModelBySlug = new Dictionary<string, PublicModelFacts>(),
        ModelOfProduct = new Dictionary<ProductId, PublicModelFacts>(),
        Values = Array.Empty<PublicProductValueFacts>().ToLookup(value => ProductId.From(1)),
        Attributes = new Dictionary<AttributeId, PublicAttributeFacts>(),
        Dictionary = new Dictionary<AttributeValueId, PublicValueFacts>(),
        Categories = new Dictionary<ProductCategoryId, PublicCategoryFacts>(),
        CategoryFilters = Array.Empty<PublicCategoryFilterFacts>().ToLookup(filter => filter.CategoryId),
        Texts = Array.Empty<PublicModelTextFacts>().ToLookup(text => text.ModelId)
    };

    private sealed class ManualClock : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = new(2026, 11, 15, 12, 0, 0, TimeSpan.Zero);
        public override DateTimeOffset GetUtcNow() => Now;
    }
}
