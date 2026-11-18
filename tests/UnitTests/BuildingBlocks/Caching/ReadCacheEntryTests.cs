using BuildingBlocks.Application.Caching;

namespace UnitTests.BuildingBlocks.Caching;

public sealed class ReadCacheEntryTests
{
    [Fact]
    public void Entry_NeedsAPositiveLifetime()
        => Assert.Throws<ArgumentOutOfRangeException>(() => ReadCacheEntry.Create("test.read", "test:tag", TimeSpan.Zero));

    [Fact]
    public void Entry_KeyCarriesTheVariant()
        => Assert.Equal("test.read:ru", ReadCacheEntry.Create("test.read", "test:tag", TimeSpan.FromSeconds(1), "ru").Key);

    [Fact]
    public void Entry_WithoutVariant_UsesItsName()
        => Assert.Equal("test.read", ReadCacheEntry.Create("test.read", "test:tag", TimeSpan.FromSeconds(1)).Key);
}
